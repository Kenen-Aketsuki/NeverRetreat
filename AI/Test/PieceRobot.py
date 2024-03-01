import torch
import torch.nn as nn
import torch.optim as optim


class StandardRNN(nn.Module):
    def __init__(self, input_size, hidden_size, num_classes):
        super(StandardRNN, self).__init__()
        self.hidden_size = hidden_size
        self.rnn = nn.RNN(input_size, hidden_size, batch_first=True)
        self.fc = nn.Linear(hidden_size, num_classes)
        self.softmax = nn.LogSoftmax(dim=2)

    def forward(self, x, hidden):
        output, hidden = self.rnn(x, hidden)
        output = self.fc(output[:, -1, :])
        output = self.softmax(output)
        return output, hidden

    def init_hidden(self, batch_size):
        return torch.zeros(1, batch_size, self.hidden_size)

    # 随机生成数据


def generate_random_data(batch_size, seq_len, input_size):
    data = torch.randn(batch_size, seq_len, input_size)
    target = torch.randint(0, 2, (batch_size, seq_len)).sum(dim=1)  # 生成随机的二分类目标
    return data, target


# 训练模型
def train_model(model, criterion, optimizer, data_loader, num_epochs):
    model.train()
    for epoch in range(num_epochs):
        total_loss = 0
        for data, target in data_loader:
            hidden = model.init_hidden(data.size(0))
            optimizer.zero_grad()
            output, _ = model(data, hidden)
            loss = criterion(output.view(-1, 2), target.long())  # 注意目标需要是long类型
            loss.backward()
            optimizer.step()
            total_loss += loss.item()
        print(f'Epoch [{epoch + 1}/{num_epochs}], Loss: {total_loss / len(data_loader):.4f}')

    # 保存模型


def save_model(model, filename):
    torch.save(model.state_dict(), filename)


# 加载模型
def load_model(model, filename):
    model.load_state_dict(torch.load(filename))
    model.eval()  # 将模型设置为评估模式


# 主程序
input_size = 10
hidden_size = 20
num_classes = 2  # 假设是二分类问题
batch_size = 4
seq_len = 3
num_epochs = 5

# 创建RNN模型
rnn = StandardRNN(input_size, hidden_size, num_classes)

# 创建优化器和损失函数
optimizer = optim.Adam(rnn.parameters(), lr=0.01)
criterion = nn.NLLLoss()

# 创建随机数据加载器
data_loader = torch.utils.data.DataLoader(
    torch.utils.data.Dataset(
        lambda: (generate_random_data(batch_size, seq_len, input_size) for _ in range(100)),  # 生成100个批次的数据
        batch_size=batch_size,
        shuffle=True
    ))

# 训练模型
train_model(rnn, criterion, optimizer, data_loader, num_epochs)

# 保存模型
save_model(rnn, 'rnn_model.pth')

# 加载模型并验证（这里只是简单地加载模型，实际上应该有一个验证数据集）
rnn.load_state_dict(torch.load('rnn_model.pth'))
print("Model loaded successfully!")

# 注意：在实际应用中，您应该使用真实的数据集和适当的数据加载器来训练模型，并进行模型验证和测试。
