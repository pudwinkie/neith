/****************************************************************************
 * 設定画面スクリプト
 *
 * $Id: setting.js 8 2010-12-18 08:50:20Z youichi $
 ****************************************************************************/

var URL_HOME	= 'http://www.plumfield.jp/~youichi/gadget/';


/****************************************************************************
 * 音声ファイル選択
 * @param no 音声ファイル番号
 */
function selectWaveFile(no)
{
	// 条件抽出
	var dir;
	var file;
	var basefile = $("#setting1 :text[name='Sound"+no+"Str']").val();
	var idx = basefile.lastIndexOf("\\");
	if(idx > 0){
		dir  = basefile.substring(0, idx);
		file = basefile.substring(idx + 1);
	} else {
		dir = '';
		file = basefile;
	}

	try{
		// ファイル選択
		var item = System.Shell.chooseFile(true, LANG["FTYPE_WAV"]+':*.wav::', dir, file);
		if(item != null){
			$("#setting1 :text[name='Sound"+no+"Str']").val(item.path);
		}
	}
	catch(e){
		// エラー無視
	}
}

/****************************************************************************
 * 最新バージョンチェック
 */
function checkVersion()
{
	/** @param text */
	function retSuccess(text){
		var msg;
		if(text == 'newver'){
			msg = LANG["CHKVER_NEW"];
		} else if(text == 'devver'){
			msg = LANG["CHKVER_DEV"];
		} else if(text.match('update,(.+)')){
			msg = LANG["CHKVER_UP"];
			msg = msg.replace('_NEWVER_', '<a href="' + URL_HOME + 'gadget.cgi?mode=download&amp;id=' + KEYNAME + '">' + RegExp.$1 + '</a>');
		} else {
			msg = '<font color="red">'+LANG["CHKVER_ERR"]+'('+text+')</font>';
		}
		$("#chkver").html(msg);
	}
	/***/
	function retError(){
		$("#chkver").html('<font color="red">'+LANG["CHKVER_NG"]+'</font>');
	}
	jQuery.ajax({
		type: "GET",
		url: URL_HOME + 'chknewver.cgi?id=' + KEYNAME + '&ver=' + System.Gadget.version,
		dataType: "text",
		success: retSuccess,
		error: retError
	});
}
