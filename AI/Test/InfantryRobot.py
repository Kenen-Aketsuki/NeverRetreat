import torch
import torch.nn as nn
import torch.optim as optim
import numpy as np
import torch.nn.functional as F

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

    def forward(self, x):
        hidden_prev = torch.zeros(1, glbSuper.num_layers, glbSuper.hidden_size)
        out,hidden_prev = self.rnn(x,hidden_prev)
        out = out.view(-1,glbSuper.hidden_size)
        out = self.liner(out)
        out = out.unsqueeze(dim = 0)
        # indices = torch.tensor([14])
        # out = torch.index_select(out,1,indices)

        return out

    def backward(self, loss):
        sto = data_storge.data_storge()
        sto.model_dict['normal'][0].zero_grad()
        loss.backward()
        sto.model_dict['normal'][0].step()

    def command_translate(self, y):
        y = torch.squeeze(y)
        if y.dim() > 1: y = y[-1,:]
        y = y.split(3)
        command_resu = F.softmax(y[0],dim=-1)
        print(y)
        dir = y[1].item()
        _,max = torch.max(command_resu,dim = -1)

        return (max.item(),dir)
#model = InfantryRobot(glbSuper.input_size,glbSuper.hidden_size,glbSuper.num_layers)
#torch.save(model.state_dict(),glbSuper.normal_model_path)

