import torch
import torch.nn as nn
import torch.optim as optim
import numpy as np
from matplotlib import pyplot as plt
import glbSuper
import data_storge

#步兵模型
output_size = 4 #输出层维度 移动、攻击、结束、方向

class InfantryRobot(nn.Module): #步兵类型

    def __init__(self,input_size,hidden_size,num_layers):
        super(InfantryRobot, self).__init__()
        self.rnn = nn.RNN( #循环神经网络层
            input_size = input_size,
            hidden_size = hidden_size,
            num_layers = num_layers,
            #[b, seq, h] 排序
            batch_first = True
        )
        for p in self.rnn.parameters():
            nn.init.normal_(p, mean=0.0, std=0.001)

        self.liner = nn.Linear(glbSuper.hidden_size,output_size) # 全连接层输出

    def forward(self, x, hidden_prev):
        out,hidden_prev = self.rnn(x,hidden_prev)
        out = out.view(-1,glbSuper.hidden_size)
        out = self.liner(out)
        out = out.unsqueeze(dim = 0)
        indices = torch.tensor([14])
        out = torch.index_select(out,1,indices)

        return out,hidden_prev

    def backward(self):

        pass

    def getLoss(self,distence,currentHP,fullHP,dealDMG):
        return distence + fullHP / currentHP - dealDMG

# model = InfantryRobot(glbSuper.input_size,glbSuper.hidden_size,glbSuper.num_layers)
# torch.save(model.state_dict(),glbSuper.normal_model_path)

