import torch
import torch.nn as nn

model_path = "Model/"
normal_model_path = "Model/normal.pth"#常规部队
mental_model_path = "Model/mental.pth" #心理战
warlock_model_path = "Model/warlock.pth" #术士
hack_model_path = "Model/hack.pth" #模组战
strick_model_path = "Model/strick.pth" #间瞄火力打击
piece_Key_path = "Data/piece_key.csv" #棋子键值路径

#超参数
num_time_step = 10 #训练时间窗的长度
input_size = 156 #输入维度
hidden_size = 300 #隐藏层维度
num_layers = 1 #隐含层层数
lr = 0.01 #学习率
dis_fix_rate = 0.5 #描述到达目标地点的距离在整体中的占比
train_times = 100 #训练的次数

# 损失函数
class CustomBattleLoss(nn.Module):
    def __init__(self):
        super(CustomBattleLoss, self).__init__()

    def forward(self, distance, currentHP, fullHP, dealDmg, enyCount, command_state):
        # 计算均方误差
        if command_state:
            loss = distance * dis_fix_rate + fullHP / currentHP - dealDmg - enyCount + 3
        else:
            loss = 100
        return torch.tensor(loss,requires_grad=True)


def step_train(model,x): #
    pass