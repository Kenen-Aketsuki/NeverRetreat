import InfantryRobot
import glbSuper
import torch
import copy
import torch.optim as optim
import csv
import numpy as np

class data_storge:
    _instance = None
    def __new__(cls):
        if not cls._instance:
            cls._instance = super(data_storge, cls).__new__(cls)

            print("init……")
            cls.piece_env = dict()  # 棋子经历的环境变动
            cls.piece_result_env = dict()  # 棋子上次决策的结果
            cls.model_dict = dict()  # 模型字典 normal-常规单位 special-有特殊行动的单位
            cls.load_model(cls)  # 加载模型
            cls.rua = "Enter init"

            cls.piece_type_dict = dict()  # 棋子兵种字典
            cls.load_piece_key(cls)

        return cls._instance

    @staticmethod
    def get_instance():
        if data_storge._instance is None:
            data_storge()  # 触发__new__方法创建单例
        return data_storge._instance

    @staticmethod
    def reset_instance():
        data_storge._instance = None
        return data_storge()

    def load_model(self):
        # 存入常规模型
        model = InfantryRobot.InfantryRobot(glbSuper.input_size, glbSuper.hidden_size, glbSuper.num_layers)
        model.load_state_dict(torch.load(glbSuper.normal_model_path))
        optimizer = optim.Adam(model.parameters(),glbSuper.lr)
        self.model_dict['normal'] = [copy.deepcopy(model),copy.deepcopy(optimizer)]

    def load_piece_key(self):
        print("BinCodeLoad")
        self.piece_type_dict.clear()
        with open(glbSuper.piece_Key_path,'r') as file:
            reader = csv.reader(file)
            for row in reader:
                self.piece_type_dict[row[0]] = row[1];

    def generate_binary_codes(self, piece_keys):
        print("BinCodeGen")
        piece_keys.insert(0, 'N/A')
        self.piece_type_dict.clear()

        # 为每个关键词生成编码
        for i, keyword in enumerate(piece_keys):
            # 将索引转换为5位二进制字符串，如果不足5位则在前面补0
            binary_code = format(i, '05b')
            self.piece_type_dict[keyword] = binary_code

        dtype = [('key', 'U30'), ('value', 'U5')]
        numpy_array = np.array(list(self.piece_type_dict.items()), dtype=dtype)

        with open(glbSuper.piece_Key_path,'w',newline='') as file:
            writer = csv.writer(file)
            writer.writerows(numpy_array)


    @staticmethod
    def piecetype_to_vector(str):
        array = []
        for i in range(len(str)):
            array.append(int(str[i]))

        return torch.from_numpy(np.array(array))

    def envdata_to_tensor(self,jsonData):
        resu = data_storge.piecetype_to_vector(
            self.piece_type_dict[jsonData.get('pieceType')]
        )

        position = jsonData.get('pos')
        position = [position.get('x'), position.get('y')]
        position = torch.from_numpy(np.array(position))
        resu = torch.cat((resu, position), dim=0)

        selfData = [jsonData.get('inCasualty'),jsonData.get('Mov'), jsonData.get('ATK'), jsonData.get('DEF')]
        selfData = torch.from_numpy(np.array(selfData))
        resu = torch.cat((resu, selfData), dim=0)

        position = jsonData.get('targetPos')
        position = [position.get('x'), position.get('y')]
        position = torch.from_numpy(np.array(position))
        resu = torch.cat((resu, position), dim=0)

        land_datas = jsonData.get('LandData')
        for land in land_datas:
            land_dat = [land.get('dir'),land.get('RRK'), land.get('ATK'), land.get('DEF'), land.get('MOV')]
            land_dat = torch.from_numpy(np.array(land_dat))
            resu = torch.cat((resu, land_dat), dim=0)

        enyList = jsonData.get('HighEffectDatas')
        for eny in enyList:
            enyTen = data_storge.piecetype_to_vector(
                self.piece_type_dict[eny.get('pieceType')]
            )
            enyDat = [eny.get('direction'), eny.get('distance'), eny.get('inCasualty'), eny.get('DEF')]
            enyDat = torch.from_numpy(np.array(enyDat))
            resu = torch.cat((resu, enyTen), dim=0)
            resu = torch.cat((resu, enyDat), dim=0)

        frdList = jsonData.get('FriendDatas')
        for frd in frdList:
            frdTen = data_storge.piecetype_to_vector(
                self.piece_type_dict[frd.get('pieceType')]
            )
            frdDat = [frd.get('direction'), frd.get('distance'), frd.get('inCasualty'), frd.get('DEF')]
            frdDat = torch.from_numpy(np.array(frdDat))
            resu = torch.cat((resu, frdTen), dim=0)
            resu = torch.cat((resu, frdDat), dim=0)

        resu = resu.unsqueeze(0)
        return resu.float()


    def get_loss(self,pieceId,command_resu):
        print("计算损失值")
        #distance, isCasualty, dealDmg, enyCount, command_state
        criterion = glbSuper.CustomBattleLoss()

        last_resu = self.piece_result_env[pieceId]
        distance = int(last_resu.get("distance")) - int(command_resu.get("distance"))
        absDistance = int(command_resu.get("distance"))
        isCasualty = int(command_resu.get("inCasualty"))
        dealDmg = int(command_resu.get("dealDmg"))
        enyCount = int(last_resu.get("enemyCount")) - int(command_resu.get("enemyCount"))
        command_state = bool(command_resu.get("commandState"))

        loss = criterion(absDistance,distance,isCasualty,dealDmg,enyCount,command_state)
        print("损失值:\n",loss)
        return loss