from flask import Flask
from flask import request
import torch

import data_storge as ds
import glbSuper

#使用：flask --app main run 启动

app = Flask(__name__)

@app.route("/")
def say_hello():
    return "OK"

@app.route("/ServeInit") # 服务端初始化
def serve_init():
    storge = ds.data_storge.get_instance()
    storge.piece_env.clear()
    return "OK"

@app.route("/UpdatePieceKey",methods = ["Post"]) # 更新棋子键编码
def update_piece_key():
    piecelist = request.json
    storge = ds.data_storge.get_instance()
    storge.generate_binary_codes(piecelist)

    return 'OK'

@app.route("/PostEnvData",methods=["Post"]) # 发送棋子的环境信息
def get_env_data():
    jsonMsg = request.json
    pieceId = jsonMsg.get('pieceId')
    storge = ds.data_storge.get_instance()
    resu = storge.envdata_to_tensor(jsonMsg)

    if pieceId in storge.piece_env:
        storge.piece_env[pieceId].append(resu)
    else:
        storge.piece_env[pieceId] = [resu]

    if len(storge.piece_env[pieceId]) > 10:
        storge.piece_env[pieceId].pop(0)

    # 解析
    batch = torch.stack(storge.piece_env[pieceId], dim=1)
    #print(batch.shape)

    # 前向传播
    resu = storge.model_dict['normal'][0].forward(batch)
    #print(resu)

    # 解析指令
    resu = storge.model_dict['normal'][0].command_translate(resu)
    #print(resu)

    return resu.__str__()

@app.route("/AiBackword",methods=["Post"]) # 发送棋子的环境信息
def backword_ai():
    storge = ds.data_storge.get_instance()
    jsonMsg = request.json
    pieceModelType = bool(jsonMsg.get('isSpecial'))
    pieceId = jsonMsg.get('pieceId')
    #print("当前棋子:" + pieceId + "\n是否为特殊模型" + pieceModelType.__str__())

    # if not pieceModelType : model = storge.model_dict["normal"][0]
    # else : model = storge.model_dict["special"][0]
    model = storge.model_dict["normal"][0]

    loss = "N/A"
    if pieceId in storge.piece_result_env:
        loss = storge.get_loss(pieceId,jsonMsg)
        model.backward(loss)

    storge.piece_result_env[pieceId] = jsonMsg
    #print(loss)

    return loss.__str__()

@app.route("/Reset")
def rest_serve():
    ds.data_storge.reset_instance()
    return "OK"

@app.route("/SaveModel")
def save_model():
    storge = ds.data_storge.get_instance()
    torch.save(storge.model_dict["normal"][0].state_dict(), glbSuper.normal_model_path)
    #torch.save(storge.model_dict["special"][0], glbSuper.special_model_path)
    return "OK"

@app.route("/PassTurn")
def pass_turn():
    print("重置数据")
    storge = ds.data_storge.get_instance()
    storge.piece_env.clear()
    storge.piece_result_env.clear()
    return "OK"