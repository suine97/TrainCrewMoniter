# TrainCrewMoniter

TrainCrewMoniterは、溝月レイル/Acty様製作の列車運転ゲーム「TRAIN CREW」で動作する、ATO/TASC機能付きモニタリングソフトです。
![1](https://github.com/user-attachments/assets/4babb6d6-8ec7-4ace-9aee-ad619d4d9dbb)

# 使い方
1、TRAIN CREWの操作設定→外部デバイス入出力を「有効」に設定してください。

2、Zipファイルを解凍後、TrainCrewMoniter.exeをダブルクリックして起動してください。

# ブレーキ曲線データ
勾配地形情報を織り込み済みのブレーキ曲線を「Bin」フォルダから読み込み、TASCの停車パターン・速度制限パターン・出力ノッチ・SAP圧力の演算に使用します。

- 配置場所：TrainCrewMoniter.exe と同じ場所の「Bin」フォルダ
- ファイル名：駅名（例：Bin\赤山町.bin）
- 該当する駅のファイルが無い場合や曲線の範囲外では、従来通り勾配情報(Xml\Gradient.xml)を用いた演算で動作します

※ブレーキ曲線データはサイズが大きいためリポジトリには含めていません。各自で「Bin」フォルダに配置してください。

# 作成情報

* 作成者:Suine97

# License
"TrainCrewMoniter" is under [MIT license](https://en.wikipedia.org/wiki/MIT_License).
