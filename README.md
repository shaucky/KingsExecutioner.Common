[![Project Version](https://img.shields.io/badge/Version-1.0.0-orange.svg)]()
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![.NET Version](https://img.shields.io/badge/.NET-Standard_2.1+-mediumpurple.svg)](https://dotnet.microsoft.com)

# 王的刽子手

《王的刽子手》是籁星（Lighsing Studio）参与TapTap 2025年的聚光灯GameJam时创作的网格式卡牌游戏。它实现了3行4列的网格战场、玩家全场共享的能量机制，以及复杂的判定节点等要素。

该仓库是《王的刽子手》的战斗系统逻辑层。受限于21天开发完整游戏的比赛时间限制，最初的策划案设计的丰富机制并未能完全落地，但完成的部分也足以构成一个有一定乐趣的玩法。

## 环境与依赖项

* 支持[.NET Standard](https://learn.microsoft.com/zh-cn/dotnet/standard/net-standard?tabs=net-standard-2-1) 2.1
和[C#](https://learn.microsoft.com/zh-cn/dotnet/csharp/whats-new/csharp-version-history#c-version-9) 9.0的环境，
例如.NET 5.0+、Unity 2021.2+。

## 设计思想

《王的刽子手》的战斗系统，由界面层和逻辑层的分层架构组成。作为纯粹的Game Jam项目来说，这有点奢侈了。但技术上来说，分层是一个好的设计。

其中，界面层由Unity负责。该仓库的代码是界面无关的逻辑层。这意味着这套战斗系统的逻辑可以再重复应用到其它游戏中——同样的核心机制、不同的体验。

为了实现这样的分层架构，我选择了使用**观察者模式**来实现战斗系统。逻辑层充当“主题”，而界面层则是“观察者”。

## 快速开始

`Combatfield`类表示一场对局，界面层可以通过该类的单例访问对局的相关成员。由于本仓库只包含界面无关的逻辑层，若要复用本仓库的代码，需要自行实现界面层及其它前置条件（例如读取配置文件并序列化为卡牌配置）的交互。

此处开源的代码不包含《王的刽子手》中实现的具体效果（因为它们是赶进度通过GPT生成的）。如果要复用该仓库的代码，需要自行实现继承自`Effect`的各种效果，并更改到`EffectFactory`的`effects`字典中。

设计上做了一定的继承。例如，对局的角色需要是`CombatParticipant`或其子类。由于开发进度原因，《王的刽子手》定义了`CombatPlayer`作为玩家，而直接使用了`CombatParticipant`作为敌人。实际上还可以设计一些Boss角色继承自`CombatParticipant`。

再比如，卡牌的根类是`CardBase`，然后，对于可以战斗的卡牌，定义了继承自前者的`CombatCard`。与此同时，并非所有的战斗卡牌都具备攻击和防守的能力，因此额外定义了`IAttacker`和`IDefender`，并有子类`FighterCard`和`ItemCard`实现所需接口。
同样是因为开发进度原因，这里的一部分定义有些“浪费”，在最终落地的《王的刽子手》中并没有意义。

…好吧，这次Game Jam给了我一个教训：真没必要在这种称不上长期的项目中做架构。但这次Jam确实做出了这样一个可以复用的战斗系统逻辑层。
