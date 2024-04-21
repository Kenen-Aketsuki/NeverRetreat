from flask import Flask
from flask import request
from flask import jsonify
import numpy as np
import torch

import data_storge as ds
import glbSuper

#使用：flask --app main run 启动

app = Flask(__name__)

@app.route("/ServeInit") # 服务端初始化
def serve_init():
    storge = ds.data_storge.get_instance()
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
    resu = ds.data_storge.envdata_to_tensor(ds.data_storge.get_instance(), jsonMsg)

    storge = ds.data_storge.get_instance()
    if pieceId in storge.piece_env:
        storge.piece_env[pieceId].append(resu)
    else:
        storge.piece_env[pieceId] = [resu]

    if len(storge.piece_env[pieceId]) > 10:
        storge.piece_env[pieceId].pop(0)

    # 解析
    batch = torch.stack(storge.piece_env[pieceId], dim=0)
    print(batch.shape)

    # 前向传播
    resu = storge.model_dict['normal'][0].forward(batch)
    print(resu)

    return "OK"

@app.route("/AiBackword",methods=["Post"]) # 发送棋子的环境信息
def backword_ai():

    pass