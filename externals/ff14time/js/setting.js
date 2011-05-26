/****************************************************************************
 * 設定画面スクリプト
 *
 * $Id: setting.js 69 2010-12-18 12:34:51Z youichi $
 ****************************************************************************/

var PAGE_BASE	= 0;
var PAGE_TIMER	= 1;
var PAGE_SHIP	= 2;
var PAGE_OTHER	= 99;

var g_bTimeDouble = false;			// 倍速表示
var g_bBaseUse = new Array();		// 基本使用フラグ
var g_bTimerUse = new Array();		// タイマー使用フラグ
var g_sTimerStr = new Array();		// タイマー設定文字列
var g_iTimerVal = new Array();		// タイマー設定値
var g_bSoundUse = new Array();		// タイマーサウンド使用
var g_sSoundStr = new Array();		// タイマーサウンドファイル
var g_bShipUse = new Array();		// 交通機関
var g_iPage = 0;					// 表示ページ
var g_bAutoTimeSet = false;			// 自動時間調整
var g_bFileSave = false;			// ファイル保存フラグ
var g_bDebugFlg = false;			// デバッグモード
var g_iVersionCheck = 0;			// バージョンチェック結果

/****************************************************************************
 * 読み込み時処理
 */
function onLoad()
{
	readSettings();
	makePageSelectBox();
	makeBase();
	makeTimerList();
	makeShip();
	makeOther();
	setLang();
	eventTimerList();
	selectPage(-1);
}

/****************************************************************************
 * 設定読み込み
 */
function readSettings()
{
	var i;
	var oConfig = loadSettings();
	g_bTimeDouble = oConfig["TimeDouble"];
	for(i = 0; i < MAX_BASE; i++){
		g_bBaseUse[i] = oConfig["Base"+i+"Use"];
	}
	for(i = 0; i < MAX_TIMER; i++){
		g_bTimerUse[i] = oConfig["Timer"+i+"Use"];
		g_iTimerVal[i] = oConfig["Timer"+i+"Sec"];
		g_sTimerStr[i] = sec2str(g_iTimerVal[i], 2);
		g_bSoundUse[i] = oConfig["Sound"+i+"Use"];
		g_sSoundStr[i] = oConfig["Sound"+i+"Str"];
	}
	for(i = 0; i < MAX_SHIP; i++){
		g_bShipUse[i] = oConfig["Ship"+i+"Use"];
	}
	g_bAutoTimeSet = oConfig["AutoTimeSet"];
	g_bFileSave = oConfig["SaveFile"];
	g_bDebugFlg = oConfig["DebugFlg"];
}

/****************************************************************************
 * ページ選択ボックス生成
 */
function makePageSelectBox(){
	// 言語情報設定
	var tabname = new Array();
	tabname[PAGE_BASE]  = "#SET_BASE#";
	tabname[PAGE_TIMER] = "#SET_TIMER#";
	tabname[PAGE_SHIP]  = "#SET_SHIP#";
	tabname[PAGE_OTHER] = "#SET_OTHER#";

	var html = "";
	for(var i in tabname){
		html += '<option class="chg_lang" value="' + i + '">' + tabname[i] + '</option>';
	}
	$("#tab select").html(html);
	$("#page").change(/***/function(){selectPage(-1);});
}

/****************************************************************************
 * 基本ページ生成
 */
function makeBase()
{
	var i;
	var listTitle = new Array("#DAY_CNT#", "#MOON_CNT#", "#GUILDLEAVE#");
	var html = '<div><label id="TimeDouble" class="chg_lang"><input type="checkbox" name="TimeDouble">#TIME_DOUBLE#</label></div>';
	for(i = 0; i < MAX_BASE; i++){
		html = html + '<div><label class="chg_lang"><input type="checkbox" name="Base' + i + 'Use">'+listTitle[i]+'</label></div>';
	}
	$("#setting0").html(html);
	$("#setting0 :checkbox[name='TimeDouble']").attr("checked", g_bTimeDouble);
	for(i = 0; i < MAX_BASE; i++){
		$("#setting0 :checkbox[name='Base"+i+"Use']").attr("checked", g_bBaseUse[i]);
	}
	$("#TimeDouble").attr("title", "#TIME_DOUBLE_MSG#");
}

/****************************************************************************
 * タイマーリストページ生成
 */
function makeTimerList()
{
	var i;
	var html = '';
	for(i = 0; i < MAX_TIMER; i++){
		html += '<tr><td>'
			+ '<label class="chg_lang"><input type="checkbox" name="Timer'+i+'Use">'+(i+1)+'#TIMERUNIT#</label>'
			+ '&nbsp;<input type="text" name="Timer'+i+'Str" size=5 maxlength=5 class="TimerStr">'
			+ '&nbsp;&nbsp;</td><td><input type="checkbox" name="Sound'+i+'Use" title="#COUNTPLAY#">'
			+ '<input type="text" name="Sound'+i+'Str" size=15 readonly class="SoundStr">'
			+ '</td><td><input type="button" name="Sound'+i+'Btn" value="..." title="#SELFILE#" onClick="selectWaveFile('+i+')">'
			+ '<img title="#PLAY#" class="playbutton" src="images/timer_sound.png"></td></tr>';
	}
	$("#setting1 table").append(html);
}

/****************************************************************************
 * タイマーリストページイベント
 */
function eventTimerList()
{
	/** @param no */
	function changeTimerCheck(no)
	{
		var bDisabledTimer;
		var bDisabledSound;
		if($("#setting1 :checkbox[name='Timer"+no+"Use']").attr("checked")){
			bDisabledTimer = false;
			bDisabledSound = !$("#setting1 :checkbox[name='Sound"+no+"Use']").attr("checked");
		} else {
			bDisabledTimer = true;
			bDisabledSound = true;
		}
		$("#setting1 :text[name='Timer"+no+"Str']").attr("disabled", bDisabledTimer);
		$("#setting1 :checkbox[name='Sound"+no+"Use']").attr("disabled", bDisabledTimer);
		$("#setting1 :text[name='Sound"+no+"Str']").attr("disabled", bDisabledSound);
		$("#setting1 :button[name='Sound"+no+"Btn']").attr("disabled", bDisabledSound);
	}

	var i;
	for(i = 0; i < MAX_TIMER; i++){
		$("#setting1 :checkbox[name='Timer"+i+"Use']").attr("checked", g_bTimerUse[i]);
		$("#setting1 :text[name='Timer"+i+"Str']").val(g_sTimerStr[i]);
		$("#setting1 :checkbox[name='Sound"+i+"Use']").attr("checked", g_bSoundUse[i]);
		$("#setting1 :text[name='Sound"+i+"Str']").val(g_sSoundStr[i]);
		changeTimerCheck(i);
	}
	$("#setting1 :checkbox[name^='Timer'][name$='Use']").each(/** @param no */function(no){
		var oThis = this;
		$(oThis).change(/***/function(){changeTimerCheck(no);});
	});
	$("#setting1 :checkbox[name^='Sound'][name$='Use']").each(/** @param no */function(no){
		var oThis = this;
		$(oThis).change(/***/function(){changeTimerCheck(no);});
	});
	$("#setting1 .playbutton").each(/** @param no */function(no){
		var oThis = this;
		$(oThis).click(/***/function(){
			if($("#setting1 :checkbox[name='Timer"+no+"Use']").attr("checked")){
				soundPlay($("#setting1 :text[name='Sound"+no+"Str']").val());
			}
		});
	});
}

/****************************************************************************
 * 交通機関ページ生成.
 */
function makeShip()
{
	var i;
	for(i = 0; i < MAX_SHIP; i++){
		$("#setting2").append('<div><label><input name="Ship' + i + 'Use" type="checkbox">' + LANG_SHIP[i] + '</label></div>');
		$("#setting2 :checkbox[name='Ship"+i+"Use']").attr("checked", g_bShipUse[i]);
	}
}

/****************************************************************************
 * その他ページ生成
 */
function makeOther()
{
	$("#AutoTimeSet :checkbox").attr("checked", g_bAutoTimeSet);
	$("#FileSave :checkbox").attr("checked", g_bFileSave);
	$("#FileSave").attr("title", LANG["FILESAVE_MSG"].replace("CONFIG_FILE", g_strConfigFilePath));
	$("#homeurl").attr("href", URL_HOME);
	$("#homeurl").text(URL_HOME);
	$("#version").text("Version " + System.Gadget.version);

	if(isDebug()){
		$("#DebugFlg").show();
		$("#DebugFlg :checkbox").attr("checked", g_bDebugFlg);
	}
}

/****************************************************************************
 * 表示ページ切り替え
 * @param page 表示ページ
 */
function selectPage(page)
{
	if(page >= 0){
		g_iPage = page;
		$("#page").val(page);
	} else {
		g_iPage = $("#page").val();
	}

	$("#setting>div").each(/***/function(){
		var oThis = this;
		if($(oThis).is("#setting"+g_iPage)){
			$(oThis).show();
		} else {
			$(oThis).hide();
		}
	});

	if(g_iPage == PAGE_OTHER){
		if(g_iVersionCheck == 0){
			checkVersion();
		}
	}
}

/****************************************************************************
 * 設定変更処理
 */
function getSettings()
{
	var i;

	// 基本
	g_bTimeDouble = $("#setting0 :checkbox[name='TimeDouble']").attr("checked");
	for(i = 0; i < MAX_BASE; i++){
		g_bBaseUse[i] = $("#setting0 :checkbox[name='Base"+i+"Use']").attr("checked");
	}

	// タイマー
	for(i = 0; i < MAX_TIMER; i++){
		g_bTimerUse[i] = $("#setting1 :checkbox[name='Timer"+i+"Use']").attr("checked");
		g_sTimerStr[i] = $("#setting1 :text[name='Timer"+i+"Str']").val();
		g_bSoundUse[i] = $("#setting1 :checkbox[name='Sound"+i+"Use']").attr("checked");
		g_sSoundStr[i] = $("#setting1 :text[name='Sound"+i+"Str']").val();
	}

	// 交通機関
	for(i = 0; i < MAX_SHIP; i++){
		g_bShipUse[i] = $("#setting2 :checkbox[name='Ship"+i+"Use']").attr("checked");
	}

	// その他
	g_bAutoTimeSet = $("#AutoTimeSet :checkbox").attr("checked");
	g_bFileSave = $("#FileSave :checkbox").attr("checked");
	g_bDebugFlg = $("#DebugFlg :checkbox").attr("checked");
}

/****************************************************************************
 * 設定書き込み
 */
function writeSettings()
{
	var i;
	var oSettings = new Object();

	// 基本
	oSettings["TimeDouble"] = g_bTimeDouble;
	for(i = 0; i < MAX_BASE; i++){
		oSettings["Base"+i+"Use"] = g_bBaseUse[i];
	}

	// タイマー
	for(i = 0; i < MAX_TIMER; i++){
		oSettings["Timer"+i+"Use"] = g_bTimerUse[i];
		oSettings["Timer"+i+"Sec"] = g_iTimerVal[i];
		oSettings["Sound"+i+"Use"] = g_bSoundUse[i];
		oSettings["Sound"+i+"Str"] = g_sSoundStr[i];
	}

	// 交通機関
	for(i = 0; i < MAX_SHIP; i++){
		oSettings["Ship"+i+"Use"] = g_bShipUse[i];
	}

	// その他
	oSettings["AutoTimeSet"] = g_bAutoTimeSet;
	oSettings["SaveFile"] = g_bFileSave;
	oSettings["DebugFlg"] = g_bDebugFlg;

	saveSettings(oSettings);
}

/****************************************************************************
 * 設定反映イベント.
 * @param event イベント
 */
function onSettingsClosing(event)
{
	if(event.closeAction == event.Action.commit){
		// 設定内容取得
		getSettings();

		// 設定内容チェック
		var i;
		var errpage = -1;
		var errflg = new Array();
		errflg[PAGE_TIMER] = new Array();

		// タイマー設定時刻チェック
		for(i = 0; i < MAX_TIMER; i++){
			if(!g_bTimerUse[i]){
				continue;
			}
			var sec = str2sec(g_sTimerStr[i]);
			if(sec > 0 && sec < 6000){
				g_iTimerVal[i] = sec;
			} else {
				errflg[PAGE_TIMER][i] = true;
				errpage = PAGE_TIMER;
			}
		}
		if(errpage == -1){ // 正常
			writeSettings();
		} else {
			selectPage(errpage);
			if(errpage == PAGE_TIMER){
				for(i = 0; i < MAX_TIMER; i++){
					var color;
					if(errflg[errpage][i]){
						color = "#ff8888";
					} else {
						color = "";
					}
					$("#setting1 :text[name='Timer"+i+"Str']").css("backgroundColor", color);
				}
			}
			event.cancel = true;
		}
	}
}

/****************************************************************************
 * ハンドラ設定
 */
window.onload = onLoad;
System.Gadget.onSettingsClosing = onSettingsClosing;
