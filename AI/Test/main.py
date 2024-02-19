from flask import Flask
from flask import request

#使用：flask --app main run 启动

app = Flask(__name__)

@app.route("/")
def hello_world():
    return "<p>Hello, World!</p>"

@app.route("/Rua")
def Get_Rua():
    return "Yeet(Not RUA)"

@app.route("/GiveRua",methods=["Post"])
def Give_Rua():
    return request.data