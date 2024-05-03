import torch
import torch.nn as nn

model_path = "Model/"
normal_model_path = "Model/normal.pth"#常规部队
special_model_path = "Model/special.pth" #心理战

piece_Key_path = "Data/piece_key.csv" #棋子键值路径

#超参数
num_time_step = 10 #训练时间窗的长度
input_size = 156 #输入维度
hidden_size = 300 #隐藏层维度
num_layers = 1 #隐含层层数
lr = 0.1 #学习率

dis_fix_rate = 0.5 #描述到达目标地点的距离在整体中的占比
hp_fix_rate = 1
errCmd_fix_rate = 2 #非法指令
total_fix_rate = 0.5#总体修正

train_times = 100 #训练的次数

# 损失函数
class CustomBattleLoss(nn.Module):
    def __init__(self):
        super(CustomBattleLoss, self).__init__()

    def forward(self,absDistance, distance, isCasualty, dealDmg, enyCount, command_state):
        # 计算均方误差
        loss = absDistance * dis_fix_rate - distance + isCasualty * hp_fix_rate - dealDmg - enyCount + 3
        if not command_state:
            loss *= errCmd_fix_rate

        loss *= total_fix_rate
        return torch.tensor(loss, dtype=torch.float, requires_grad=True)


def step_train(model,x): #
    pass