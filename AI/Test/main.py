from flask import Flask
from flask import request
from flask import jsonify

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
    nam = request.json.get("Rua")

    if nam == "Rua":
        response = jsonify(dict(Rest=request.json.get("yeet") + nam))
    else:
        response = jsonify(dict(Rest= nam + ",你就是个几把！"))
    return response