<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN" "http://www.w3.org/TR/html4/loose.dtd">
<html>
<head>
	<meta http-equiv="Content-type" content="text/html; charset=utf-8">
	<title><?php echo(wfMsg('Page.Install.page-title'));?></title>
	
	<link href="/config/assets/install.css" rel="stylesheet" type="text/css" />
	
	<script type="text/javascript">
		//<![CDATA[
		var uri = location.protocol + '//' + location.host + '/config/assets/install_js.css';
		if(document.createStyleSheet) {
			document.createStyleSheet(uri);
		}
		else {
			var styles = "@import url(' " + uri + " ');";
			var newSS=document.createElement('link');
			newSS.rel='stylesheet';
			newSS.href='data:text/css,'+escape(styles);
			document.getElementsByTagName("head")[0].appendChild(newSS);
		}
		fieldsetArray = new Array();
		<?php
		if ($wgIsVM)
		{
			echo 'fieldsetArray = ["form-admin-title","form-title","tell-us"];';
		}
		else
		{
			echo 'fieldsetArray = ["form-admin-title","form-title","form-dbconfig","form-adv","tell-us"];';
		}
		?>
		//]]>
	</script>
	<script type="text/javascript" src="/skins/common/jquery/jquery.min.js"></script>
	<script type="text/javascript" src="/config/assets/install.js"></script>

</head>

<body class="install-<?php echo get_installation_method(true); ?>">

<div class="wrap">
	<div class="logo">
		<img src="/skins/common/logo/logo.png" alt="MindTouch" />
	</div>
	<h1><?php echo(wfMsg('Page.Install.page-title'));?></h1>