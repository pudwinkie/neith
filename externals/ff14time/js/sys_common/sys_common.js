/****************************************************************************
 * 共通処理
 *
 * $Id: common.js 7 2010-12-18 08:04:24Z youichi $
 ****************************************************************************/

System.Gadget.path.match(/.*\\([0-9a-z]+)/);
var KEYNAME		= RegExp.$1;
var CONF_FILE	= KEYNAME + "_conf.json";	// 設定ファイル
var LOG_FILE	= KEYNAME + ".log";

jQuery.ajaxSetup({
	cache: false,
	timeout: 5000
});

var g_strConfigFilePath;
try {
	g_strConfigFilePath = System.Environment.getEnvironmentVariable('USERPROFILE')+"\\"+CONF_FILE;
}
catch(e)
{
	// エラー無視
}

/****************************************************************************
 * 秒を形式変換
 * @param sec 秒
 * @param fmt 変換パターン(2:「分:秒、3:「時:分:秒」)
 * @return {String} 変換結果
 */
function sec2str(sec,fmt)
{
	var secstr = sec;
	if(fmt == 2){
		secstr = zeropad(Math.floor(sec / 60))
		+ ":" + zeropad(sec % 60);
	}
	if(fmt == 3){
		secstr =
			zeropad(Math.floor(sec / 3600)) + ":" +
			zeropad(Math.floor((sec % 3600) / 60)) + ":" +
			zeropad(sec % 60);
	}
	return secstr;
}

/****************************************************************************
 * 0パディング(2桁)
 * @param val 数値
 * @return {String} 整形結果
 */
function zeropad(val)
{
	if(val <= 9){
		val = "0" + val;
	}
	return val;
}

/****************************************************************************
 * 「分:秒」形式を秒に変換
 * @param strsec 時間文字列
 * @return 変換結果(-1:エラー)
 */
function str2sec(strsec)
{
	var i;
	var time_m = 0;
	var time_s = 0;
	var mode = 0;
	for(i = 0; i < strsec.length; i++){
		var code = strsec.charCodeAt(i);
		if(mode == 0){ // 分を取得
			if(code == 0x3a && i > 0){ // :
				mode = 1;
				continue;
			}
			if(code >= 0x30 && code <= 0x39){ // 0～9
				time_m = time_m * 10 + code - 0x30;
			} else {
				return -1;
			}
		}
		if(mode > 0){ // 秒を取得
			mode = 2;
			if(code >= 0x30 && code <= 0x39){ // 0～9
				time_s = time_s * 10 + code - 0x30;
			} else {
				return -1;
			}
		}
	}
	if(mode != 2){
		return -1;
	}
	return time_m * 60 + time_s;
}

/****************************************************************************
 * 音再生
 * @param file 音ファイル
 */
function soundPlay(file)
{
	if(file != ""){
		try{
			System.Sound.playSound(file);
		}
		catch(e){
			// エラー無視
		}
	}
}

/****************************************************************************
 * 言語情報反映
 */
function setLang()
{
	/**
	 * 置換
	 * @param strAll
	 * @param strMatch
	 * @return r {String}
	 */
	function replacement(strAll, strMatch){
		if(LANG[strMatch] == undefined){
			return "?("+strMatch+")";
		}
		return LANG[strMatch];
	}

	$(".chg_lang").each(/***/function(){
		var oThis = this;
		$(oThis).html($(oThis).html().replace(/#([0-9A-Z_]+)#/, replacement));
	});
	$("[title^=#]").each(/***/function(){
		var oThis = this;
		$(oThis).attr("title", $(oThis).attr("title").replace(/^#([0-9A-Z_]+)#$/, replacement));
	});
}

/****************************************************************************
 * 設定書き込み.
 * @param oConfig 保存オブジェクト
 * @return 保存結果 {Boolean}
 */
function saveSettings(oConfig)
{
	var bRet = true;
	var oStream = new ActiveXObject("Scripting.FileSystemObject");
	oConfig["InitFlg"] = true;
	var strConfigJSON = $.toJSON(oConfig);

	if(oConfig["SaveFile"]){
		// ファイルに保存
		try{
			var oFile = oStream.OpenTextFile(g_strConfigFilePath, 2, true, -2);
			oFile.Write(strConfigJSON);
			oFile.Close();
		}
		catch(e){
			bRet = false;
		}
	} else {
		// ガジェット設定に保存
		System.Gadget.Settings.writeString("AllConfig", strConfigJSON);
		try {
			oStream.DeleteFile(g_strConfigFilePath);
		}
		catch(e){
			// エラー無視
		}
	}
	return bRet;
}

/****************************************************************************
 * 設定読み込み.
 * @return 設定情報 {*}
 */
function loadSettings()
{
	var oStream = new ActiveXObject("Scripting.FileSystemObject");
	var strConfigJSON = "";
	try {
		var oFile = oStream.OpenTextFile(g_strConfigFilePath, 1, false, -2);
		strConfigJSON = oFile.ReadAll();
		oFile.Close();
	}
	catch(e){
		// ファイルから取得できないときはガジェット設定から取得
		strConfigJSON = System.Gadget.Settings.readString("AllConfig");
	}
	var oConfig;
	try {
		oConfig = $.evalJSON(strConfigJSON);
	}
	catch(e){
		oConfig = {};
	}
	return oConfig;
}

/****************************************************************************
 * デバッグモードチェック
 * @return {Boolean} デバッグモード許可
 */
function isDebug()
{
	var ret = false;
	// ログファイルがあるときだけデバッグモードを有効にする
	var obj = new ActiveXObject("Scripting.FileSystemObject");
	if(obj.FileExists(System.Environment.getEnvironmentVariable('USERPROFILE')+"\\"+LOG_FILE)){
		ret = true;
	}
	return ret;
}
