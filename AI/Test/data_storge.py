import InfantryRobot
import glbSuper
import torch
import copy


class data_storge:
    _instance = None


    def __init__(self):
        self.piece_env = dict() #棋子经历的环境变动
        self.piece_result = dict() #棋子上次决策的结果
        self.model_dict = dict()

        #存入常规模型
        model = InfantryRobot(glbSuper.input_size,glbSuper.hidden_size,glbSuper.num_layers)
        model.load_state_dict(torch.load(glbSuper.model_path + "test_model.pth"))
        self.model_dict['normal'] = copy.deepcopy(model)

    def __new__(cls):
        if not cls._instance:
            cls._instance = super(cls)
        return cls._instance



    def load_model(self):

        pass