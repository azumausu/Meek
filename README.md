# Meek
Meekは、DIベースで実装されたUnityのユーザーインターフェイスを構築するためのライブラリです。  
主な機能としては、画面遷移や画面ライフサイクルの管理機能を提供しています。  
また、MVPアーキテクチャでの実装を容易にするためのツールも提供しています。
このライブラリを使用することで、クリーンな実装やUnity C#とPure C#を分離した実装の実現が容易になります。

[MeekDemo](https://user-images.githubusercontent.com/19426596/232242080-f2eac6e7-e1ae-48c3-9816-8aebae1f951b.mov)

デモの中で使用している画像はフリーのコンテンツです。    
著作権に関する情報は以下のウェブサイトをご確認ください。  
[Nucleus UI](https://www.nucleus-ui.com/)

## Requirements
* Unity 2021.3 (LTS) or newer

## Getting Started
Package Managerを開いて`Add Package from git URL...`から下記のパッケージを追加してください。
```
https://github.com/azumausu/Meek.git?path=Assets/Packages
```

* [NavigationStack](./Assets/Packages/Meek.NavigationStack/README_JA.md) - 画面遷移 + 画面管理機能 を使用したい場合
* [Meek MVP](./Assets/Packages/Meek.MVP/README_JA.md) - 画面遷移 + 画面管理機能 + MVPアーキテクチャ を使用したい場合


## License
Meek is licensed under the MIT License.
