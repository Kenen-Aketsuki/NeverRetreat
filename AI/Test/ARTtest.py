import torch
import torch.nn.functional as F


class ART1(torch.nn.Module):
    def __init__(self, input_size, vigilance_param=0.7, max_categories=2):
        """
        初始化ART1网络
        :param input_size: 输入向量的大小
        :param vigilance_param: 警戒参数，用于决定何时创建新的类别
        :param max_categories: 网络可以学习的最大类别数
        """
        super(ART1, self).__init__()
        self.input_size = input_size
        self.vigilance_param = vigilance_param
        self.max_categories = max_categories
        # 类别权重矩阵，初始化为随机值
        self.categories = torch.nn.Parameter(torch.randn(max_categories, input_size))
        # 标记网络是否已初始化
        self.initialized = False

    def forward(self, x):
        """
        前向传播函数，用于训练和预测
        :param x: 输入向量
        :return: 预测的类别索引
        """
        if not self.initialized:
            # 如果网络未初始化，则使用前几个输入向量作为初始类别
            self.initialize_categories(x)
            self.initialized = True
        return self.predict(x)

    def predict(self, x):
        """
        预测输入向量所属的类别
        :param x: 输入向量
        :return: 预测的类别索引
        """
        # 计算输入向量与每个类别的相似度
        similarities = torch.mm(x, self.categories.t())
        # 获取相似度最高的类别索引
        _, indices = torch.max(similarities, dim=1)
        return indices

    def initialize_categories(self, x):
        """
        使用输入数据的前几个向量初始化类别
        :param x: 输入数据
        """
        # 选择前几个向量作为初始类别
        self.categories.data = x[:self.max_categories]

    def update_categories(self, x, indices):
        """
        根据输入向量和获胜类别的索引更新类别
        :param x: 输入向量
        :param indices: 获胜类别的索引列表
        """
        # 根据索引更新类别权重
        for i in range(self.max_categories):
            if indices[i] is not None:
                self.categories[i] = x[indices[i]]

    def train(self, x):
        """
        训练函数，接收输入向量并更新网络
        :param x: 输入向量
        """
        # 前向传播，如果必要则更新类别
        self.forward(x)

    # 使用示例


if __name__ == "__main__":
    # 创建ART1网络实例
    art_network = ART1(input_size=4, vigilance_param=0.7, max_categories=5)

    # 训练数据
    training_data = torch.tensor([
        [1, 0, 1, 0],
        [1, 1, 0, 0],
        [0, 1, 0, 1],
        [0, 1, 1, 0],
        [0, 0, 1, 1]
    ], dtype=torch.float32)

    # 训练网络
    for vector in training_data:
        # 对每个输入向量进行训练
        predicted_category = art_network(vector.view(1, -1))
        print(f"输入向量 {vector} 预测的类别索引: {predicted_category.item()}")

        # 测试数据
    test_data = torch.tensor([
        [1, 0, 1, 0],
        [0, 1, 1, 1],
        [1, 1, 1, 0]
    ], dtype=torch.float32)

    # 测试网络
    for vector in test_data:
        # 对每个测试向量进行预测
        predicted_category = art_network(vector.view(1, -1))
        print(f"测试向量 {vector} 预测的类别索引: {predicted_category.item()}")