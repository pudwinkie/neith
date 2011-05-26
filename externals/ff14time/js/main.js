/****************************************************************************
 * メインスクリプト
 *
 * $Id: main.js 89 2010-12-19 11:36:27Z youichi $
 ****************************************************************************/

// 定数
var URL_CGI			= 'http://www.plumfield.jp/~youichi/cgi/' + KEYNAME + '.cgi';

var TIMECNV_DIFF	= 1278950400;	// エオルゼア時間に変換する前に減算する値
var TIMECNV_GAME	= 144;			// エオルゼアの単位時間(86400:1日)
var TIMECNV_EARTH	= 7;			// 地球の単位時間(4200:70分)

// グローバル変数
var g_strLogFilePath = "";			// ログファイルフルパス
var g_oRefreshTime;					// 時間更新タイマー
var g_oRefreshAll;					// 全体更新タイマー
var g_iPutDate;						// 表示済みの日(1日1回の更新用)

var g_iLogLine = 0;					// ログ出力行数
var g_DgbData = 0;					// デバッグ用
var g_bGadgetFlg = true;			// ガジェットとして動作している

var g_oConfig = {};					// 設定

/****************************************************************************
 * 初期設定
 */
function init()
{
	var i;
	try {
		System.Gadget.settingsUI = "setting.html";
//		System.Gadget.onDock = function () { /* サイズを大きくする */ };
//		System.Gadget.onUndock = function () { /* サイズを小さくする */ };

		/** @param event */
		System.Gadget.onSettingsClosed = function (event)
		{
			if(event.closeAction == event.Action.commit){
				g_oConfig = loadSettings();
				clearTimeout(g_oRefreshTime);
				clearTimeout(g_oRefreshAll);
				viewChange();
				refreshTime();
				refreshAll();
			}
		};

		g_strLogFilePath = System.Environment.getEnvironmentVariable('USERPROFILE')+"\\"+LOG_FILE;
	}
	catch(e)
	{
		g_bGadgetFlg = false; // ブラウザとして動作している
	}

	// 初期値設定
	if(g_bGadgetFlg){
		g_oConfig = loadSettings();
	}
	else{
		// ブラウザモードでの表示指定
		g_oConfig["InitFlg"] = true;
		g_oConfig["TimeDouble"] = false;	// 倍速表示
		g_oConfig["Base0Use"] = true;		// 翌日まで
		g_oConfig["Base1Use"] = true;		// 月情報
	}
	if(!g_oConfig["InitFlg"]){
		g_oConfig = {};

		g_oConfig["TimeDouble"] = false;	// 倍速表示
		g_oConfig["Base0Use"] = true;		// 翌日まで
		g_oConfig["Base1Use"] = true;		// 月情報
		g_oConfig["Base2Use"] = false;		// ギルドリーヴ

		g_oConfig["Ship0Use"] = false;		// リムサ・ロミンサ⇔ザナラーン

		g_oConfig["Timer0Sec"] = 120;
		g_oConfig["Timer1Sec"] = 180;
		g_oConfig["Timer2Sec"] = 300;
		g_oConfig["Timer3Sec"] = 600;
		g_oConfig["Timer4Sec"] = 1800;
		g_oConfig["Timer5Sec"] = 60;
		g_oConfig["Timer6Sec"] = 60;
		g_oConfig["Timer7Sec"] = 60;

		// 有効な音ファイルをさがす
		var windir = System.Environment.getEnvironmentVariable('windir');
		var file = "";
		var obj = new ActiveXObject("Scripting.FileSystemObject");
		var soundfiles = new Array("ringin.wav", "notify.wav");
		for(var f in soundfiles){
			if(obj.FileExists(windir+"\\Media\\"+soundfiles[f])){
				file = windir+"\\Media\\"+soundfiles[f];
			}
		}

		for(i = 0; i < MAX_TIMER; i++){
			g_oConfig["Sound"+i+"Str"] = file;
		}

		saveSettings(g_oConfig);
	}

	viewInit();
	setLang();
	viewChange();
	refreshTime();
	refreshAll();
	$("#body2").show();
}

/****************************************************************************
 * 表示初期化
 */
function viewInit()
{
	initTimer();
}

/****************************************************************************
 * 表示全体更新
 */
function viewChange()
{
	var i;

	// 基本表示
	var confid = new Array("daycount", "mooninfo", "guildleave");
	for(i = 0; i < MAX_BASE; i++){
		if(g_oConfig["Base"+i+"Use"]){
			$("#"+confid[i]).show();
		} else {
			$("#"+confid[i]).hide();
		}
	}

	// タイマー基礎表示
	viewTimer();

	// 船タイムテーブル
	for(i = 0; i < MAX_SHIP; i++){
		if(g_oConfig["Ship"+i+"Use"]){
			$("#ship"+i+"view").show();
			$("#ship"+i+"view .dest").text(LANG_SHIP[i]);
		} else {
			$("#ship"+i+"view").hide();
		}
	}

	// 自動時刻修正
	viewAutoTime();

	// デバッグ表示
	if(g_oConfig["DebugFlg"]){
		$("#debug").show();
	} else {
		$("#debug").hide();
	}
	putLog(LOG_INF, "viewChange()");

	adjustHeight();
}

/****************************************************************************
 * 時間のみ更新
 */
function refreshTime()
{
//	 エオルゼア1日は地球の70分
//	 エオルゼアは32日で1ヶ月
//	 エオルゼア4日で月齢が変わり、8回で1週
	g_DgbData++;

	// エオルゼアの表示周期(秒)
	var nTimeSpan = g_oConfig["TimeDouble"] ? 10 : 20;

	// 元になる時間を取得
	var fJstTime = getAdjustTime();
	var iETime = getGameTime(fJstTime / 1000);

	// 更新タイミング調整
	var ntime = nTimeSpan - iETime % nTimeSpan;
	if(ntime < 5){
		ntime += nTimeSpan;
	}
	g_oRefreshTime = setTimeout("refreshTime()", ntime * TIMECNV_EARTH / TIMECNV_GAME * 1000);

	// 端数を四捨五入
	iETime = Math.round(iETime / 10) * 10;

	// エオルゼアの時間を表示
//	var iEYear = Math.floor(iETime / 33177600);
	var iEMon  = Math.floor(iETime % 33177600 / 2764800) + 1;
	var iEDay  = Math.floor(iETime % 2764800 / 86400) + 1;
	var iEHour = Math.floor(iETime % 86400 / 3600);
	var iEMin  = Math.floor(iETime % 3600 / 60);
	var iESec  = iETime % 60;
	var sVDate = zeropad(iEMon) + "/" + zeropad(iEDay);
	var sVTime = zeropad(iEHour) + ":" + zeropad(iEMin) + ":" + zeropad(iESec);
	$("#vDate").text(sVDate);
	$("#vTime").text(sVTime);
}

/****************************************************************************
 * 定期的な表示内容の更新
 */
function refreshAll()
{
	g_DgbData += 1000;

	// 元になる時間を取得
	var iJstTime = getAdjustTime();
	var iMiliSec = iJstTime % 1000;
	iJstTime = Math.round(iJstTime / 1000);

	// 更新タイミング調整
	var ntime = Math.floor(1000 - iMiliSec);
	if(ntime < 300){
		ntime += 1000;
	}
	g_oRefreshAll = setTimeout("refreshAll()", ntime);

	var iETime = getGameTime(iJstTime);
	var iEDate = Math.floor(iETime / 86400);

	if(iEDate != g_iPutDate){ // エオルゼア1日1回
		g_iPutDate = iEDate;
		g_DgbData += 100000000;
		if(g_oConfig["Base1Use"]){
			// 月の形は4日ごとに変わって、32日で1周する。
			var nMoonNo = Math.floor(iETime / (86400 * 4)) % 8;
			$("#moonico").attr({"src": "images/moon"+nMoonNo+".png", "title" : LANG["MOON"+nMoonNo]});
		}
	}

	if(g_oConfig["Base0Use"]){ // 翌日まで
		$("#daycount0str").text(sec2str(countDown(false, iJstTime, 1440, 0), 2));
	}
	if(g_oConfig["Base1Use"]){ // 月情報表示
		$("#mooncount0str").text(sec2str(countDown(false, iJstTime, 4*1440, 0), 3));
		$("#mooncount1str").text(sec2str(countDown(false, iJstTime, 32*1440, 0), 3));
		$("#mooncount2str").text(sec2str(countDown(false, iJstTime, 32*1440, 16*1440), 3));
	}
	if(g_oConfig["Base2Use"]){ // ギルドリーヴ
		$("#guildleavestr").text(sec2str(countDown(true, iJstTime, 36*60, 0), 3));
	}
	if(g_oConfig["Ship0Use"]){ // 交通機関(リムサ⇔ザナラーン)
		$("#ship0b").text(sec2str(countDown(true, iJstTime, 10, 9.75), 2));
//		$("#ship0c").text(sec2str(countDown(true, iJstTime, 10, 9.75), 2));
	}

	// デバッグ情報表示
	if(g_oConfig["DebugFlg"]){
		var msec = '00'+Math.floor(iMiliSec);
		msec = msec.slice(-3);
		var dbgtxt = '<div>';
		dbgtxt += iJstTime + "." + msec + "</br>";
		dbgtxt += "ETime=" + iETime + "</br>";
		dbgtxt += "next=" + ntime + "</br>";
		dbgtxt += "diff=" + Math.round(g_fDiffTime) + "," + Math.floor(g_fTimeChkDis) + ","+ g_nTimeChkSkipMax + "</br>";
		dbgtxt += "Data=" + g_DgbData + "</div>";
		$("#debugdata").html(dbgtxt);
	}
}
