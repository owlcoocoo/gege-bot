# 简介

一个以 C# 编写的机器人，基于 [go-cqhttp](https://github.com/Mrs4s/go-cqhttp) 的接口。

功能：

- [ ] pixiv
  
  - [x] 关键字或id搜图
  
  - [x] 关键字或id搜画师
  
  - [x] 生成预览图
  
  - [x] 根据id搜图搜画师
  
  - [x] 同个id分批发送（某些作品放了上百张图片）
  
  - [x] 排行榜
  
  - [ ] 指定分页
  
  - [ ] 随机作品且不重复
  
  - [x] pixivision 每日插画特辑推送

- [x] 自动同意好友请求/群邀请

- [ ] 关键字梗图

- [ ] 群聊复读

- [x] 戳一戳发送随机图片

- [x] 接入 [llama.cpp](https://github.com/ggerganov/llama.cpp)

- [x] 接入 [EdgeGPT](https://github.com/Integration-Automation/ReEdgeGPT)

- [x] 使用语言模型回复

- [x] 群聊匹配词库关键字自动回复

# 部署

配置文件是 `config.json`，请确保程序目录下有此文件。

本项目使用的是 .NET 8.0 框架，依赖于 .NET 8 运行库。

如需源码运行请下载 .NET 8 SDK。

参考：

[下载 .NET 8.0 (Linux、macOS 和 Windows)](https://dotnet.microsoft.com/zh-cn/download/dotnet/8.0)

[在 Windows、Linux 和 macOS 上安装 .NET | Microsoft Learn](https://learn.microsoft.com/zh-cn/dotnet/core/install/)
