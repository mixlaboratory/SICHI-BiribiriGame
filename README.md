参考にしたWebページ：https://qiita.com/amanatsutouko/items/4e08979e1a6da7790a24

下記URLからJoyconLi06.unitypacageをダウンロード
https://github.com/Looking-Glass/JoyconLib/releases

アセットからダウンロードしたパッケージをインポート

下は

Joy-Con操作設定のまとめ
<img width="1340" height="1685" alt="image" src="https://github.com/user-attachments/assets/fb42e978-7a45-4a29-b52d-0fc7238237f2" />


移動できる最大範囲は，次の項目です．

Maximum Pointer Offset

現在は15です．

5  ：かなり狭い
10 ：やや狭い
15 ：現在の設定
20 ：広い
25 ：かなり広い

画面端まで届かない場合は，20程度へ上げます．

感度を変更する項目

基本的な感度は，次の項目です．

Pointer Sensitivity

現在は3なので，かなり敏感な設定です．

0.5：低感度
1.0：標準
1.5：少し高感度
2.0：高感度
3.0：かなり高感度

細かな操作を重視する場合は，1.0～1.5程度が扱いやすいです．

Pointer Distanceでも感度は変わりますが，まずはPointer Distanceを10に固定し，Pointer Sensitivityだけで調整すると分かりやすいです．

動作の遅れを変更する項目

Joy-Conを動かした後，玉が遅れて付いてくる場合は，

Follow Speed

を上げます．

10：ゆっくり
30：標準
50：素早い
100：ほぼ即時

反応を良くしたい場合は，30から40～50へ上げます．

手ぶれを抑える項目

静止していても玉が細かく揺れる場合は，

Pointer Dead Zone

を上げます．

0.01：細かな動きにも反応する
0.03：現在の設定
0.05：やや安定する
0.10：かなり安定するが，小さな操作に反応しにくい

イライラ棒の細かな操作では，0.03～0.05程度がよいです．

振動の強さ

振動の強さは，

Rumble Strength

で設定します．

0.0：振動なし
0.2：弱い
0.5：中程度
0.8：現在の設定，強め
1.0：最大

衝突時のフィードバックとして使用する場合は，0.4～0.6程度から試すとよいです．

振動時間

振動時間は，

Rumble Milliseconds

で設定します．

100：0.1秒
200：0.2秒
300：0.3秒
500：0.5秒
1000：1秒

壁に触れた瞬間のフィードバックには，200～300ミリ秒程度が自然です．

イライラ棒用の推奨初期設定
Pointer Distance：10
Pointer Sensitivity：1.2～1.5
Pointer Dead Zone：0.03～0.05
Maximum Pointer Offset：15～20
Follow Speed：40
Swap Axes：オフ
Invert X：必要に応じて設定
Invert Y：必要に応じて設定

Rumble Strength：0.5
Rumble Milliseconds：250
Rumble Low Frequency：160
Rumble High Frequency：320

まずはこの設定で試し，移動範囲が足りなければMaximum Pointer Offsetを上げ，動きが大きすぎる場合はPointer Sensitivityを下げる，という順番で調整すると分かりやすいです．
