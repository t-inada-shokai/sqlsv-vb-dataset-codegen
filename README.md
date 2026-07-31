# sqlsv-vb-dataset-codegen

SQL Serverのテーブル・テーブル型・ストアドプロシジャの定義を読み取り、VB.NET側の

- 強く型付けされたDataSet(`.xsd`)
- ストアドプロシジャ呼び出し用のラッパークラス(`.vb`)

を自動生成するためのSQL Serverストアドプロシジャ／バッチファイル／VB.NETプロジェクト一式です。

「DBスキーマの定義を唯一の正とし、アプリ側のコードは生成物として扱う」という考え方のもと、テーブルやストアドの定義が変わった際に手作業でVB側のコードを直す手間をなくすことを目的としています。

解説記事: (ここにQiita記事のURLを挿入)

## できること

1. **テーブル/テーブル型 → DataSet(XSD)生成**
   `C_`, `D_`, `M_`, `R_`, `P_`, `GEO_`, `type_` の命名規則に沿ったテーブル・ユーザー定義テーブル型の構造を読み取り、Visual Studioの`DataSet`デザイナで使える`.xsd`を生成します。
2. **ストアドプロシジャ → VB.NET呼び出しクラス生成**
   `adm_`/`private_`以外のストアドプロシジャの引数定義を読み取り、`SqlCommand`/`SqlParameter`の組み立てを内包した呼び出し用クラス(`Proc_ストアド名`)を生成します。テーブル型のOUTPUTパラメータについては、一時テーブル経由で結果セットとして受け渡す仕組みを自動で組み込みます。

## リポジトリ構成

```
.
├── LICENSE
├── README.md
├── SQLSvFiles/                              … SQL Server側の資産
│   ├── DBsettingForBatch.ini                … バッチ共通のDB接続設定(値は要編集)
│   ├── Src00_01CreateDB/
│   │   └── 001CreateDB.sql                  … サンプル用DB作成スクリプト
│   ├── Src01_01CreateTable/                 … サンプルテーブル・テーブル型
│   │   ├── SaveTable.bat
│   │   ├── tbl_D_SAMPLE_DATA.sql
│   │   ├── typ_TYPE_D_SAMPLE_DATA.sql
│   │   └── *_list.txt                       … 対象オブジェクト一覧(バックアップ用途)
│   ├── Src01_00_15VB_xsd作成用/             … ★DataSet(XSD)生成一式
│   │   ├── SaveTableInfo.bat                … 実行バッチ
│   │   ├── ConvertXMLtoXSD_STDINOUT.exe     … 中間XML→XSD変換
│   │   ├── ReshapeXSD_STDINOUT.exe          … XSD属性の並び順整形
│   │   ├── DSSQLSV.xml / .xml.temp / .xsd   … 生成物のサンプル
│   │   └── ReadMe.txt
│   └── Src01_00_17VB_StoredClass作成用/     … ★呼び出しクラス生成一式
│       ├── SaveClass.bat                    … 実行バッチ
│       └── AppCommonsClass_DBAccess_Stored.vb … 生成物のサンプル
└── VBSolution/                               … VB.NET側の資産(Visual Studioソリューション)
    ├── SQLSv主体のVBとの連携.sln
    ├── Library_AppTemplate/                  … 共通基盤ライブラリ(DLL)
    ├── Project_ConvertXMLtoXSD_STDINOUT/     … ConvertXMLtoXSD_STDINOUT.exe のソース
    ├── Project_ReshapeXSD_STDINOUT/          … ReshapeXSD_STDINOUT.exe のソース
    └── Project_SampleApp/                    … 生成物を実際に使うサンプルアプリ
```

> **Note:** ストアドプロシジャ定義SQL作成・テーブル定義SQL作成・ビュー/トリガー/ファンクション定義SQL作成など、DBオブジェクトのDDLをバックアップ的にファイル化する管理用ストアド群は、本リポジトリのスコープ外のため含めていません。

## 動作環境

- SQL Server (`sqlcmd`コマンドが利用できる環境)
- .NET Framework 4.8.1
- Visual Studio (DataSetデザイナ・カスタムツール実行に使用)
- VB.NETプロジェクトは以下のNuGetパッケージに依存しています
  - `Costura.Fody` (参照DLLの単一exeへの埋め込み)
  - `System.Text.Json`

## セットアップ手順

### 1. サンプルDB・テーブルの作成

```
SQLSvFiles/Src00_01CreateDB/001CreateDB.sql を実行してDBを作成
SQLSvFiles/Src01_01CreateTable/ のSQLでサンプルテーブル・テーブル型を作成
```

### 2. 接続設定

`SQLSvFiles/DBsettingForBatch.ini` を編集し、接続先を設定してください(値はサンプルとして伏字にしてあります)。

```ini
HOSTNAME=192.168.x.x
DBNAME=workspace
DBloginID=<your_login_id>
DBpassword=<your_password>
```

### 3. DataSet(XSD)の生成

```
SQLSvFiles/Src01_00_15VB_xsd作成用/SaveTableInfo.bat を実行
→ DSSQLSV.xsd が生成される
→ VB.NETプロジェクト内の DSSQLSV.xsd に上書きコピー
→ ソリューションエクスプローラーで DSSQLSV.xsd を右クリック →「カスタムツールの実行」
→ DSSQLSV.Designer.vb が更新される
```

### 4. ストアド呼び出しクラスの生成

```
SQLSvFiles/Src01_00_17VB_StoredClass作成用/SaveClass.bat を実行
→ AppCommonsClass_DBAccess_Stored.vb が生成される
→ VB.NETプロジェクトにコピー(上書き)
```

### 5. サンプルアプリで動作確認

`VBSolution/Project_SampleApp` に、上記の生成物を実際に使ったサンプル(`TEST_STOREDPROCEDURE`ストアドの呼び出し)が含まれています。

## ライセンス

MIT License. `LICENSE`ファイルを参照してください。

## Author

稲田商会
