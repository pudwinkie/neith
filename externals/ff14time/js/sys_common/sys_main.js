/****************************************************************************
 * 共通メイン
 *
 * $Id: main.js 14 2010-12-18 15:31:15Z youichi $
 ****************************************************************************/

var URL_TIMESET		= 'http://time.plumfield.jp:8080/nowtime';
var LOG_ERR			= 1;
var LOG_INF			= 2;
var TIMECHK_SIZE	= 10;

var g_iTimerEnd = new Array();			// タイマー終了時間
var g_oTimerInt = new Array();			// タイマー割り込みオブジェクト
var g_oDiffTime;						// 自動時刻修正タイマー
var g_fDiffTime = 0;					// 自動時刻修正値
var g_bTimeChkList = new Array();		// 日時チェック結果
var g_fTimeChkDiff = new Array();		// 日時チェック差分
var g_nTimeChkCnt = 0;					// 日時チェック回数
var g_fTimeChkDis = 0;					// 日時チェックずれ安定度(分散)
var g_nTimeChkSkip = 0;					// 日時チェックスキップ残り数
var g_nTimeChkSkipMax = 1;				// 日時チェックスキップ最大数
var g_nTimeChkLast = 0;					// 前回日時チェック時間

/****************************************************************************
 * タイマー初期化.
 */
function initTimer()
{
	// タイマー基礎表示
	for(var i = 0; i < MAX_TIMER; i++){
		g_iTimerEnd[i] = 0;
		var timerinfo = '<img id="sound'+i+'use" title="#COUNTPLAY#" class="sound" src="images/clear.png" width="15" height="15">'
			+ '<img title="#START#" class="start" src="images/timer_run.png">'
			+ '<img title="#STOP#" class="stop" src="images/timer_stop.png">'
			+ '<span id="timer'+i+'str">00:00</span>';
		if(i == 0){
			$("#timer0").html(timerinfo);
		} else {
			$("#timer"+(i - 1)).after('<div id="timer'+i+'" class="timer">'+timerinfo+'</div>');
		}
	}
	$("div.timer .start").each(/** @param no */function(no){
		var oThis = this;
		$(oThis).click(/***/function(){timerStart(no);});
	});
	$("div.timer .stop").each(/** @param no */function(no){
		var oThis = this;
		$(oThis).click(/***/function(){timerStop(no);});
	});
}

/****************************************************************************
 * タイマー全体更新.
 */
function viewTimer()
{
	for(var i = 0; i < MAX_TIMER; i++){
		if(g_oConfig["Timer"+i+"Use"]){
			$("#timer"+i).show();
			if(g_oTimerInt[i]){
				continue;
			}
			var soundimg;
			if(g_oConfig["Sound"+i+"Use"]){
				soundimg = "images/timer_sound.png";
			} else {
				soundimg = "images/clear.png";
			}
			$("#sound"+i+"use").attr("src", soundimg);
			timerStop(i);
		} else {
			$("#timer"+i).hide();
			timerStop(i);
		}
	}
}

/****************************************************************************
 * 高さ設定と即時表示更新
 */
function adjustHeight()
{
	// 高さ変更のタイミングがずれることがあるのでしばらく周期的に調整
	g_iPutDate = 0;
	var interval = setInterval(/***/function (){
		$("body").height($("#body").outerHeight() + 1);
	}, 100);
	setTimeout(/***/function(){
		clearInterval(interval);
	}, 1000);
}

/****************************************************************************
 * 自動時刻修正.
 */
function viewAutoTime()
{
	clearInterval(g_oDiffTime);
	if(g_oConfig["AutoTimeSet"]){
		autoTimeSet();
		g_oDiffTime = setInterval("autoTimeSet()", 10000);
		g_nTimeChkCnt = 0;
		$("#AutoTimeFlg").show();
	} else {
		$("#AutoTimeFlg").hide();
	}
}

/****************************************************************************
 * 現在日時取得.
 * @return 現在日時(ミリ秒)
 */
function getAdjustTime()
{
	var iJstTime = new Date().getTime() + g_fDiffTime;
//	var iJstTime = TIMECNV_DIFF * 1000; // 基準日(スナップショット用)
	return iJstTime;
}

/****************************************************************************
 * ゲーム時間に変換
 * @param time 地球時間(秒)
 * @return ゲーム時間(秒)
 */
function getGameTime(time)
{
	return Math.round((time - TIMECNV_DIFF) * TIMECNV_GAME / TIMECNV_EARTH);
}

/****************************************************************************
 * カウントダウン
 * @param earth 地球時間で計算
 * @param time  現在時刻(地球時間)
 * @param cycle 周期(分,対象先時間)
 * @param diff  ずれ(分,対象先時間)
 * @return {Number} 残り秒数
 */
function countDown(earth, time, cycle, diff)
{
	// 単位変換(分→秒)
	cycle *= 60;
	diff *= 60;

	var timeLeft;

	if(earth){
		// 地球時間での残り時間
		timeLeft = cycle - (time - diff) % cycle;
	} else {
		// 対象先の残り秒数
		timeLeft =  Math.round((cycle - (getGameTime(time) - diff) % cycle) * TIMECNV_EARTH / TIMECNV_GAME);
	}

	return timeLeft;
}


/****************************************************************************
 * タイマー開始.
 * @param iTNo タイマー番号
 */
function timerStart(iTNo)
{
	if(g_oTimerInt[iTNo]){
		timerStop(iTNo);	// 古いタイマーを削除
	}
	g_oTimerInt[iTNo] = setInterval("timerUpdate("+iTNo+")", 1000);
	g_iTimerEnd[iTNo] = getAdjustTime() + g_oConfig["Timer"+iTNo+"Sec"] * 1000;
	timerUpdate(iTNo);
	$("#timer"+iTNo).addClass("run");
}

/****************************************************************************
 * タイマー終了.
 * @param iTNo タイマー番号
 */
function timerStop(iTNo)
{
	clearInterval(g_oTimerInt[iTNo]);
	if(g_oConfig["Timer"+iTNo+"Use"]){
		$("#timer"+iTNo+"str").text(sec2str(g_oConfig["Timer"+iTNo+"Sec"], 2));
		$("#timer"+iTNo).removeClass("run");
	}
	g_oTimerInt[iTNo] = null;
	g_iTimerEnd[iTNo] = 0;
}

/****************************************************************************
 * タイマー更新
 * @param iTNo タイマー番号
 */
function timerUpdate(iTNo)
{
	var iJstTime = getAdjustTime();
	if(g_iTimerEnd[iTNo] > iJstTime + 500){
		$("#timer"+iTNo+"str").text(sec2str(Math.round((g_iTimerEnd[iTNo] - iJstTime) / 1000), 2));
	} else if(g_iTimerEnd[iTNo] > iJstTime - 500){
		$("#timer"+iTNo+"str").text('00:00');
		if(g_oConfig["Sound"+iTNo+"Use"]){
			soundPlay(g_oConfig["Sound"+iTNo+"Str"]);
		}
	} else {
		timerStop(iTNo);
	}
}

/****************************************************************************
 * 自動時刻修正
 */
function autoTimeSet()
{
	var timefast = new Date().getTime();

	// 呼び出し周期が狂ったときは日時安定度をクリア
	if(Math.abs(timefast - g_nTimeChkLast - 10000) > 100){
		g_nTimeChkSkipMax = 1;
	}
	g_nTimeChkLast = timefast;

	// 日時が安定しているときはチェックスキップ
	if(g_nTimeChkSkip > g_nTimeChkSkipMax){
		g_nTimeChkSkip = g_nTimeChkSkipMax;
	}
	g_nTimeChkSkip--;
	if(g_nTimeChkSkip > 0){
		return;
	}
	g_nTimeChkSkip = g_nTimeChkSkipMax;

	var i;

	/** @param state */
	function setAutotimeState(state){
		var title;
		var file;

		// 接続の安定度を計測
		g_bTimeChkList[g_nTimeChkCnt % TIMECHK_SIZE] = state;
		g_fTimeChkDiff[g_nTimeChkCnt % TIMECHK_SIZE] = g_fDiffTime;
		g_nTimeChkCnt++;
		var nCntMax = (g_nTimeChkCnt < TIMECHK_SIZE) ? g_nTimeChkCnt : TIMECHK_SIZE;
		var nCntOk = 0;
		for(i = 0; i < nCntMax; i++){
			if(g_bTimeChkList[i]){
				nCntOk++;
			}
		}
		var fCntPer = nCntOk / nCntMax;
		if(fCntPer >= 0.75){
			title = LANG["AUTOTIME_OK"];
			file  = "autotime_ok";
		} else if(fCntPer >= 0.1){
			title = LANG["AUTOTIME_OK"];
			file  = "autotime_wrn";
		} else {
			title = LANG["AUTOTIME_ERR"];
			file  = "autotime_err";
		}
		$("#AutoTimeFlg>img").attr({"title": title, "src" : "images/"+file+".png"});

		// ずれの安定度を計測
		if(g_nTimeChkCnt >= TIMECHK_SIZE){
			var fDiffAvr = 0;
			for(i = 0; i < TIMECHK_SIZE; i++){
				fDiffAvr += g_fTimeChkDiff[i];
			}
			fDiffAvr /= TIMECHK_SIZE;
			g_fTimeChkDis = 0;
			for(i = 0; i < TIMECHK_SIZE; i++){
				g_fTimeChkDis += Math.pow(fDiffAvr - g_fTimeChkDiff[i], 2);
			}
			g_fTimeChkDis /= TIMECHK_SIZE;
			if(g_fTimeChkDis > 60000){
				g_nTimeChkSkipMax = 1;
			} else if(g_nTimeChkSkipMax < 60 && g_fTimeChkDis < 2500){
				g_nTimeChkSkipMax++;
			} else if(g_nTimeChkSkipMax > 1 && g_fTimeChkDis > 10000){
				g_nTimeChkSkipMax--;
			}
		}
	}

	/** @param svtime */
	function retSuccess(svtime){
		var timediff = new Date().getTime() - timefast;
		if(timediff < 500){
			g_fDiffTime = svtime * 1000 - timediff / 2 - timefast;
			putLog(LOG_INF, "autotime ok g_fDiffTime="+g_fDiffTime);
			setAutotimeState(true);
		} else {
			putLog(LOG_ERR, "autotime timeout");
			setAutotimeState(false);
		}
	}

	/** @param oReq */
	function retError(oReq){
		putLog(LOG_ERR, "autotime status error [" + oReq.status + ":" + oReq.statusText + "]");
		setAutotimeState(false);
		g_nTimeChkSkipMax = 1;
	}

	jQuery.ajax({
		type: "GET",
		url: URL_TIMESET,
		dataType: "text",
		success: retSuccess,
		error: retError
	});
}

/****************************************************************************
 * ログ出力
 * @param iLv  出力レベル
 * @param sMsg 出力文字列
 */
function putLog(iLv, sMsg)
{
	if(!g_oConfig["DebugFlg"]){
		return;
	}

	// 時間文字列生成
	var iDate = new Date();
	var iYear = iDate.getFullYear();
	var iMon  = iDate.getMonth() + 1;
	var iDay  = iDate.getDate();
	var iHour = iDate.getHours();
	var iMin  = iDate.getMinutes();
	var iSec  = iDate.getSeconds();
	var sLv = '---';
	if(iLv == LOG_ERR){
		sLv = 'ERR';
	}
	if(iLv == LOG_INF){
		sLv = 'INF';
	}
	var iMsec = '00'+iDate.getMilliseconds();
	iMsec = iMsec.slice(-3);

	// 行生成
	var logmsg = iYear+"/"+zeropad(iMon)+"/"+zeropad(iDay)+" "+zeropad(iHour)+":"+zeropad(iMin)+":"+zeropad(iSec)
		+"."+zeropad(iMsec)+" "+sLv+" "+sMsg+"\n";

	if(g_iLogLine == 0){
		var obj = new ActiveXObject("Scripting.FileSystemObject");
		try{
			obj.DeleteFile(g_strLogFilePath+".old");
		}
		catch(e){
			// エラー無視
		}
		try{
			obj.MoveFile(g_strLogFilePath, g_strLogFilePath+".old");
		}
		catch(e){
			// エラー無視
		}
	}
	g_iLogLine = (g_iLogLine + 1) % 5000;

	// ファイル出力
	var oStream = new ActiveXObject("Scripting.FileSystemObject");
	try{
		var oFile = oStream.OpenTextFile(g_strLogFilePath, 8, true, 0);
		oFile.Write(logmsg);
		oFile.Close();
	}
	catch(e){
		// エラー無視
	}
}
