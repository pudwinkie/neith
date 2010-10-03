	<?php
	$userlang = wfCurrentUserLanguage();
	$languages = wfAvailableResourcesLanguages();
	$userlocalization = !array_key_exists($userlang, $languages)? 'en-us': $userlang;
$departments = array('' => wfMsg('Page.Install.tell-us-select'), 'Engineering' => 'Engineering', 'Product' => 'Product', 'IT' => 'IT', 'Marketing' => 'Marketing', 'Finance' => 'Finance', 'Operations' => 'Operations', 'Sales' => 'Sales', 'Multiple' => 'Multiple', 'Other' => 'Other');
$select_users = array('0' => wfMsg('Page.Install.tell-us-select'), '25' => '0 - 25', '150' => '26 - 150', '500' => '151 - 500', '2500' => '501 - 2500', '2501' => '2500+');
$howtouse = array('wiki', 'collaboration', 'mashups', 'intranet', 'extranet', 'knowledge-base', 'community', 'dms', 'project-management', 'portal', 'si', 'sharepoint', 'dashboard', 'dev-platform', 'reporting', 'other');
?>
	
<form action="index.php" name="config" id="config" method="post">
	<div class="navigation" display="none">
		<?php
		$fieldSetItems = $wgIsVM ?
			array('form-admin-title','form-title','tell-us') :
			array('form-admin-title','form-title','form-dbconfig', 'form-adv', 'tell-us');
	
		foreach($fieldSetItems as $key => $field)
	{
			addNavItem($key, $field);
	}
	?>
	</div>


<fieldset id="form-admin-title">
	<legend><?php echo(wfMsg('Page.Install.form-admin-title'));?></legend>
	
	<div class="table">
		<?php
		echo(installFormField(
			'SysopName',
			wfMsg('Page.Install.form-adminname'), 
			wfInputForm('text', 'SysopName', $conf->SysopName)
		));
		echo(installFormField(
			'SysopPass',
			wfMsg('Page.Install.form-adminpwd'), 
			wfInputForm('password', 'SysopPass', $conf->SysopPass)
		));
		echo(installFormField(
			'SysopPass2',
			wfMsg('Page.Install.form-adminpwd2'), 
			wfInputForm('password', 'SysopPass2', $conf->SysopPass2)
		));
		
		echo(installFormField(
			'SysopEmail',
			wfMsg('Page.Install.form-adminmail'), 
			wfInputForm('text', 'SysopEmail', $conf->SysopEmail),
			wfMsg('Page.Install.form-adminmail-desc')
		));
		?>
		
	</div>

	<div class="navButtons">
		<div class="nextButton">
			<input type="button" onclick="nextSet(0)" value="<?php echo wfMsg('Page.Install.nav-next'); ?>" class="submit_form">
		</div>
	</div>
</fieldset>


<fieldset id="form-title">
	<legend><?php echo(wfMsg('Page.Install.form-title'));?></legend>
	<p><?php echo(wfMsg('Page.Install.form-title-desc')); ?></p>

	<div class="table">
		<?php
		echo(installFormField(
			'EmergencyContact',
			wfMsg('Page.Install.form-siteemail'),
			wfInputForm('text', 'EmergencyContact', $conf->EmergencyContact),
			wfMsg('Page.Install.form-siteemail-desc')
		));
		echo(installFormField(
			'RegistrarFirstName',
			wfMsg('Page.Install.form-admin-first'),
			wfInputForm('text', 'RegistrarFirstName', $conf->RegistrarFirstName)
		));
		echo(installFormField(
			'RegistrarLastName',
			wfMsg('Page.Install.form-admin-last'),
			wfInputForm('text', 'RegistrarLastName', $conf->RegistrarLastName)
		));
		echo(installFormField(
			'Sitename',
			wfMsg('Page.Install.form-sitename'),
			wfInputForm('text', 'Sitename')
		));
		echo(installFormField(
			'RegistrarPhone',
			wfMsg('Page.Install.form-adminphone'), 
			wfInputForm('text', 'RegistrarPhone', $conf->RegistrarPhone),
			wfMsg('Page.Install.phone')
		));
		echo(installFormField(
			'RegistrarDept',
			wfMsg('Page.Install.form-department'),
			wfSelectForm('RegistrarDept', $departments)
		));
		echo(installFormField(
			'SiteLang',
			wfMsg('Page.Install.form-localization'),
			wfSelectForm('SiteLang', $languages, $userlocalization)
		));
		echo(installFormField(
			'RegistrarCount',
			wfMsg('Page.Install.tell-us-ppl'),
			wfSelectForm('RegistrarCount', $select_users)
		));
				
		?>
	</div>

	<div class="navButtons">
		<div class="backButton">
			<input type="button" onclick="previousSet(1)" value="<?php echo wfMsg('Page.Install.nav-back'); ?>" class="submit_form">
		</div>
		<div class="nextButton">
			<input type="button" onclick="nextSet(1)" value="<?php echo wfMsg('Page.Install.nav-next'); ?>" class="submit_form">
		</div>
	</div>
</fieldset>

<?php if(!$wgIsVM) { ?>
<fieldset id="form-dbconfig">
	<legend><?php echo(wfMsg('Page.Install.form-dbconfig'));?></legend>
	<?php
	if ($wgIsMSI) {
		echo('<div class="existing"><p>'.wfMsg('Page.Install.form-mysql').'</p></div>');
	}
	?>
	
	<div class="table">
	<?php
	
	echo(installFormField(
		'DBserver',
		wfMsg('Page.Install.form-dbhost'), 
		wfInputForm( 'text', 'DBserver', $conf->DBserver), 
		wfMsg('Page.Install.form-dbhost-desc'))
	);
	
	echo(installFormField(
		'DBname',
		wfMsg('Page.Install.form-dbname'), 
		wfInputForm( 'text', 'DBname', $conf->DBname), 
		wfMsg('Page.Install.form-dbname-desc'))
	);
	
	
	echo(installFormField(
		'DBuser',
		wfMsg('Page.Install.form-dbuser'), 
		wfInputForm( 'text', 'DBuser', $conf->DBuser), 
		wfMsg('Page.Install.form-dbuser-desc'))
	);
	
	echo('<div class="existing"><h2>'.wfMsg('Page.Install.form-dbexist-title').'</h2>');
	echo('<p>'.wfMsg('Page.Install.form-dbexist').'</p></div>');

	echo(installFormField(
		'RootUser',
		wfMsg('Page.Install.form-dbsu-name'), 
		wfInputForm( 'text', 'RootUser', $conf->RootUser))
	);
	
	echo(installFormField(
		'RootPW',
		wfMsg('Page.Install.form-dbsu-pwd'),
		wfInputForm( 'password', 'RootPW', $conf->RootPW))
	);
	?>
	</div>

	<div class="navButtons">
		<div class="backButton">
			<input type="button" onclick="previousSet(2)" value="<?php echo wfMsg('Page.Install.nav-back'); ?>" class="submit_form">
		</div>
		<div class="nextButton">
			<input type="button" onclick="nextSet(2)" value="<?php echo wfMsg('Page.Install.nav-next'); ?>" class="submit_form">
		</div>
	</div>
</fieldset>

<fieldset id="form-adv">
	<legend><?php echo(wfMsg('Page.Install.form-adv'));?></legend>
	
	<div class="table">
	<?php
	
	if (!$wgIsMSI) 
	{
		echo(installFormField(
			'Mono',
			wfMsg('Page.Install.form-adv-mono'), 
			wfInputForm('text', 'Mono', $conf->Mono), 
			wfMsg('Page.Install.form-adv-mono-desc')
		));
	}
	
	if (!$wgIsMSI) 
	{
		echo(installFormField(
			'ImageMagickConvert',
			wfMsg('Page.Install.form-adv-convert'),
			wfInputForm( 'text', 'ImageMagickConvert', $conf->ImageMagickConvert), 
			wfMsg('Page.Install.form-adv-convert-desc')
		));
		
		echo(installFormField(
			'ImageMagickIdentify',
			wfMsg('Page.Install.form-adv-identify'), 
			wfInputForm('text', 'ImageMagickIdentify', $conf->ImageMagickIdentify), 
			wfMsg('Page.Install.form-adv-identify-desc')
		));
		
		echo(installFormField(
			'prince',
			wfMsg('Page.Install.form-adv-prince'), 
			wfInputForm('text', 'prince', $conf->prince), 
			wfMsg('Page.Install.form-adv-prince-desc')
		));
	}
	else 
	{
		echo(wfInputForm('hidden', 'ImageMagickConvert', $conf->ImageMagickConvert));
		echo(wfInputForm('hidden', 'ImageMagickIdentify', $conf->ImageMagickIdentify));
		echo(wfInputForm('hidden', 'prince', $conf->prince));
		echo('<p><em>'.wfMsg('Page.Install.installer-autodetect').'</em></p>');
	}
	?>
	</div>

	<div class="navButtons">
		<div class="backButton">
			<input type="button" onclick="previousSet(3)" value="<?php echo wfMsg('Page.Install.nav-back'); ?>" class="submit_form">
		</div>
		<div class="nextButton">
			<input type="button" onclick="nextSet(3)" value="<?php echo wfMsg('Page.Install.nav-next'); ?>" class="submit_form">
		</div>
	</div>
</fieldset>

<?php } // end wgIsVM 

?>

<fieldset id="tell-us">
	<legend><?php echo(wfMsg('Page.Install.tell-us'));?></legend>
	<p><?php echo(wfMsg('Page.Install.tell-us-description')); ?></p>
	
	<p style="margin-bottom:0; padding-bottom: 0;">
		<strong><?php echo(wfMsg('Page.Install.tell-us-how')); ?></strong>
	</p>
	<ul class="usagelist"><?php
	$values = isset($_POST['RegistrarUsage']) ? $_POST['RegistrarUsage']: array(); 
	foreach ($howtouse as $type) {
		echo('<li>'.wfInputForm('checkbox', 'RegistrarUsage['.$type.']', 1, array('checked' => array_key_exists($type, $values)), wfMsg('Page.Install.how-'.$type)).'</li>');
	}
	?></ul>
	<div class="navButtons">
		<div class="backButton">
			<input type="button" onclick="previousSet(<?php echo ($wgIsVM) ? '2' : '4';?>)" value="Back" class="submit_form">
		</div>
	</div>
</fieldset>

<div class="submit">
	<input type="submit" value="<?php echo(wfMsg('Page.Install.submit'));?>" class="btn-install" />
</div>

</form>

<script type="text/javascript">
	$('#config').submit(function() {
		return validateSubmit();
	});
</script>

<?php
function installFormField($id, $tag, $input, $desc = '')
{
	$tag = '<div class="key">'.$tag.':</div>';
	$desc .= '<span id="invalid_'.$id.'" class="error" style="display: none;">'.wfMsg('Page.Install.required').'</span>';
	/*if (!empty($label))
	{
		$key = '<label for="'.$label.'">'.$key.'</label>';	
	}*/
	return $tag . '<div class="input">' . $input . '</div><div class="description">' . $desc . '</div>';
	}

function addNavItem($key, $field)
{
	?>
	<div onclick="javascript:gotoItem(<?php echo $key?>)" class="nav_item">
		<div id="box_<?php echo $key?>"><?php echo ($key+1)?></div>
		<span><?php echo(wfMsg('Page.Install.'.$field));?></span>
	</div>
	<?php
}

