model_path = "Model/"
normal_model_path = "Model/normal.pth"#常规部队
mental_model_path = "Model/mental.pth" #心理战
warlock_model_path = "Model/warlock.pth" #术士
hack_model_path = "Model/hack.pth" #模组战
strick_model_path = "Model/strick.pth" #间瞄火力打击

#超参数
num_time_step = 10 #训练时间窗的长度
input_size = 76 #输入维度
hidden_size = 152 #隐藏层维度
num_layers = 2 #隐含层层数
lr = 0.01 #学习率

def step_train(model,x): #
    pass