<?php
# MindTouch Deki web-based config/installation
# Copyright (C) 2004 Brion Vibber <brion@pobox.com>, 2006 Rob Church <robchur@gmail.com>
# http://www.mindtouch.com/
#
# This program is free software; you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation; either version 2 of the License, or
# (at your option) any later version.
#
# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
# GNU General Public License for more details.
#
# You should have received a copy of the GNU General Public License along
# with this program; if not, write to the Free Software Foundation, Inc.,
# 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
# http://www.gnu.org/copyleft/gpl.html

# Define required versions of key components of stack
define('REQUIRED_PHP_VERSION', '5.0.0');
define('REQUIRED_MYSQL_VERSION', '5.0.0');
define('REQUIRED_APACHE_VERSION', '2.0.0');

# Used to generate LocalSettings
define('NEWLINE', "\n");

# Do not modify these file values
define('FILE_STARTUP_XML', 'mindtouch.deki.startup.xml');
define('FILE_STARTUP_XML_IN', 'mindtouch.deki.startup.xml.in');
define('FILE_LOCALSETTINGS', 'LocalSettings.php');
define('FILE_HOST_XML', 'mindtouch.host.conf');
define('FILE_HOST_XML_IN', 'mindtouch.host.conf.in');
define('FILE_HOST_WIN_BAT', 'mindtouch.host.bat');
define('FILE_HOST_WIN_BAT_IN', 'mindtouch.host.bat.in');

/***
 * DEFINE PHP SETTINGS THAT CAUSE THE INSTALLER TO QUIT
 * MT royk: these are remnants from MW, but I'm sure they're equally applicable to DW
 * Key is the PHP settings, value is the URL which explains them (used in localization string)
 */
$fatal_settings_if_enabled = array( 
	'magic_quotes_runtime' => 'http://www.php.net/manual/en/ref.info.php#ini.magic-quotes-runtime', 
	'magic_quotes_sybase' => 'http://www.php.net/manual/en/ref.sybase.php#ini.magic-quotes-sybase', 
	'mbstring.func_overload' => 'http://www.php.net/manual/en/ref.mbstring.php#mbstring.overload', 
	'zend.ze1_compatibility_mode' => 'http://www.php.net/manual/en/ini.core.php'
);

/***
 * DEFINE PHP FUNCTIONS, IF MISSING, CAUSE THE INSTALLER TO QUIT
 * Key is the PHP function, value is the localization string key
 */
$fatal_functions_if_disabled = array( 
	'utf8_encode' => 'Page.Install.check-xml-fail', 
	'mb_strtoupper' => 'Page.Install.check-mb-fail', 
	'session_name' => 'Page.Install.check-session-fail', 
	'preg_match' => 'Page.Install.check-preg-fail', 
	'mysql_connect' => 'Page.Install.check-mysql-fail', 
	'curl_init' => 'Page.Install.check-curl-fail',
	'gd_info' => 'Page.Install.check-gd-fail', 
);

/***
 * DEFINE PHP SETTINGS, IF ENABLED, CAUSE A VISUAL WARNING
 * Key is the PHP settings, value is the URL which explains them (used in localization string)
 */
$warn_settings = array(
	'register_globals' => 'http://php.net/register_globals', 
	'safe_mode' => 'http://www.php.net/features.safe-mode'
);

/***
 * DEFINE SUPPLEMENTARY APPLICATIONS, IF MISSING, CAUSE A VISUAL WARNING ASKING TO SET THE VALUE
 * Key is the app name, value is part of the $conf, as well as used in localization key
 * Keys are converted for different OS: For windows, they are converted to \{command}.exe, for others, /{command}
 * See install_get_paths() to see how base paths are detected
 */
$warn_functionality_settable_paths = array(
	'identify' => 'ImageMagickIdentify', 
	'convert' => 'ImageMagickConvert', 
	'prince' => 'prince', //todo: implement UI
	'mono' => 'Mono', //a special case will catch this and not execute for windows platforms
);

/***
 * DEFINE APACHE MODULES, IF MISSING, CAUSE INSTALLER TO QUIT
 */
$fatal_apache_modules_if_disabled = array( 
	'mod_rewrite', 
	'mod_proxy'
);

//-------------------------------------------------------------------------------------//
error_reporting( E_ALL );
header( "Content-type: text/html; charset=utf-8" );
@ini_set( "display_errors", true );

define('MINDTOUCH_DEKI', true);
define("MEDIAWIKI_INSTALL", true);

# Attempt to set up the include path, to fix problems with relative includes
$IP = dirname( dirname( __FILE__ ) );
define( 'MW_INSTALL_PATH', $IP );
$sep = PATH_SEPARATOR;
$dsep = DIRECTORY_SEPARATOR;
if( !ini_set( "include_path", ".".$sep.$IP.$sep.$IP.$dsep."includes".$sep.$IP.$dsep."deki".$sep.$IP.$dsep."languages" ) ) {
	set_include_path( ".".$sep.$IP.$sep.$IP.$dsep."includes".$sep.$IP.$dsep."languages" );
}

$wgConfiguring = true;

//installation-specific includes
require_once( "maintenance/install-utils.inc" );
require_once( "maintenance/install-helpers.inc" );

/***
 * Check PHP version;
 * We do this check independently from other package checking because it may throw errors on included files
 * If failed, this script will stop here. 
 */
if (!install_php_version_checks()) 
{
	exit();
}

require_once( "includes/Defines.php" );
require_once( "includes/DefaultSettings.php" );
require_once( "core/deki_namespace.php" );
require_once( "includes/GlobalFunctions.php" );
require_once( "includes/Hooks.php" );

// Localization 
require_once( 'languages/Language.php' );

// Default settings
require_once( 'init.php' );

//set default paths to binaries

if( file_exists( "$IP/config/vm.php" ))
{
    require_once( "$IP/config/vm.php" );
}

if( file_exists( "$IP/config/msi.php" ))
{
    require_once( "$IP/config/msi.php" );
}

if( file_exists( "$IP/config/installtype.ami.php" ))
{
    require_once( "$IP/config/installtype.ami.php" );
}

if( file_exists( "$IP/config/installtype.package.php" ))
{
    require_once( "$IP/config/installtype.package.php" );
}

if( file_exists( "$IP/config/installtype.vmesx.php" ))
{
    require_once( "$IP/config/installtype.vmesx.php" );
}

if( file_exists( "$IP/config/enterprise.php" ))
{
    require_once( "$IP/config/enterprise.php" );
}

wfSetDefaultPaths();

// mySQL is the only supported database
$ourdb = array(
	'mysql' => array(
		'fullname' => 'MySQL', 
		'havedriver' => 0, 
		'compile' => 'mysql', 
		'rootuser' => 'root'
	)
);
$DefaultDBtype = 'mysql';

//-------------------------------------------------------
//output buffer starts here
include('config/skin.header.php');

/* Check for existing configurations and bug out! */
if ( $wgIsMSI ) 
{
	if ( file_exists( "../LocalSettings.php" ) && filesize( "../LocalSettings.php" ) > 0 ) 
	{
		dieout(wfMsg('Page.Install.setup-complete'));
	} 
}
else 
{
	if ( file_exists( "../LocalSettings.php" ) ) 
	{
		dieout(wfMsg('Page.Install.setup-complete'));
	}
}

/* Verify this folder is writable */
if( !is_writable( "." ) ) 
{
	dieout( "<h2>Can't write config file, aborting</h2>

	<p>In order to configure the wiki you have to make the <tt>config</tt> subdirectory
	writable by the web server. Once configuration is done you'll move the created
	<tt>LocalSettings.php</tt> to the parent directory, and for added safety you can
	then remove the <tt>config</tt> subdirectory entirely.</p>

	<p>To make the directory writable on a Unix/Linux system:</p>

	<pre>
	cd <i>/path/to/wiki</i>
	chmod a+w config
	</pre>
	
	<p>After fixing this, please reload this page.</p>" );
}

echo('<fieldset id="environment"><legend>'.wfMsg('Page.Install.ui-check-environment').'</legend>'
	.'<div id="env-check" class="env-check"><p class="header">'.wfMsg('Page.Install.ui-copy-errors').'</p>'
	.'<ul class="env-check">');

$conf = new ConfigData;
error_reporting( 0 );

//FAIL for missing settings/packages/libs
if (!install_verify_databases($ourdb) 
	|| !install_apps_version_check()
	|| !install_verify_php_functions($fatal_functions_if_disabled, $fatal_settings_if_enabled)
 	|| !install_verify_apache_modules($fatal_apache_modules_if_disabled)) 
{
	echo_fail(wfMsg('Page.Install.error-fatal'));
	dieout('');
}
error_reporting( E_ALL );

//for warnings, only execute this codepath for non-posted responses
if (strcmp($_SERVER["REQUEST_METHOD"], 'POST') != 0) 
{
	//WARN for certain PHP settings
	install_warn_php_settings($warn_settings, $conf);
	
	//WARN for missing packages
	install_warn_packages($warn_functionality_settable_paths, $conf);
	
	//remnants from mediawiki: do we need this?
	install_session_path();
	
	//remnants from mediawiki: do we need to do this? 
	install_raise_memory_limit();
}
//todo: html2ps and ps2pdf $conf setting
//todo: handle this better

print '</ul></div>'
	.'<div class="success"><div>'.wfMsg('Page.Install.can-install').'</div></div>'
	.'</fieldset>';
	
//set configuration
$conf->IP = dirname( dirname( __FILE__ ) );
// PHP_SELF isn't available sometimes, such as when PHP is CGI but
// cgi.fix_pathinfo is disabled. In that case, fall back to SCRIPT_NAME
// to get the path to the current script... hopefully it's reliable. SIGH
$conf->ScriptPath = preg_replace( '{^(.*)/config.*$}', '$1', ($_SERVER["PHP_SELF"] === '') ? $_SERVER["SCRIPT_NAME"]: $_SERVER["PHP_SELF"] );
$conf->posted = ($_SERVER["REQUEST_METHOD"] == "POST");
$conf->Sitename = ucfirst( importPost( "Sitename", "MindTouch" ) );
$conf->SiteLang = importPost( "SiteLang" );
$conf->EmergencyContact = importPost( "EmergencyContact", '' );
// bug 7876; if they don't provide the server email, use the administrator's email
if (empty($conf->EmergencyContact)) {
	$conf->EmergencyContact = importPost( "SysopEmail", ''); 
}
$conf->DBtype = importPost( "DBtype", $DefaultDBtype );
$conf->ApiKey = generateKey(32);
$conf->Guid = md5($conf->ApiKey);
$conf->PathPrefix = "@api";
$conf->IpAddress = "localhost";
$conf->HttpPort = "8081";
$conf->LuceneStore =  $wgLucenePath;
$conf->DBserver = importPost( "DBserver", "localhost" );
$conf->DBname = importPost( "DBname", "wikidb" );
$conf->DBuser = importPost( "DBuser", "wikiuser" );
$conf->DBpassword = importPost( "DBpassword" );
$conf->SysopName = importPost( "SysopName", "Admin" );
$conf->SysopEmail = importPost( "SysopEmail", '' );
$conf->SysopPass = importPost( "SysopPass" );
$conf->SysopPass2 = importPost( "SysopPass2" );
$conf->RootUser = importPost( "RootUser", "root" );
$conf->RootPW = importPost( "RootPW", "" );
$useRoot = importCheck( 'useroot', true );
$conf->ImageMagickConvert = importPost( "ImageMagickConvert", isset($conf->ImageMagickConvert) ? $conf->ImageMagickConvert: '');
$conf->ImageMagickIdentify = importPost( "ImageMagickIdentify", isset($conf->ImageMagickIdentify) ? $conf->ImageMagickIdentify: '');
$conf->Mono = importPost( "Mono", isset($conf->Mono) ? $conf->Mono: '');
$conf->prince = importPost( "prince", isset($conf->prince) ? $conf->prince: '');
$conf->RegistrarFirstName = importPost( "RegistrarFirstName" );
$conf->RegistrarLastName = importPost( "RegistrarLastName" );
$conf->RegistrarPhone = importPost( "RegistrarPhone" );
$conf->RegistrarCountry = importPost( "RegistrarCountry" );
$conf->RegistrarCount = importPost( "RegistrarCount" );
$conf->RegistrarDept = importPost( "RegistrarDept" );
$conf->RegistrarUsage = isset($_POST['RegistrarUsage']) ? $_POST['RegistrarUsage']: ''; 

if (is_array($conf->RegistrarUsage)) {
	$conf->RegistrarUsage = implode(', ', array_keys($conf->RegistrarUsage));	
}

// hard-code some values for the VM or the MSI
if ($wgIsVM || $wgIsMSI) 
{
	if ($wgIsVM) 
	{
		$conf->DBserver = 'localhost';
		$conf->RootUser = $wgDBadminuser;
		$conf->RootPW = $wgDBadminpassword;
	    $conf->Mono = $wgPathMono;
    }
    $conf->prince = $wgPathPrince;
    $conf->ImageMagickConvert = $wgPathConvert;
    $conf->ImageMagickIdentify = $wgPathIdentify;
}

/* Check for validity */
$errs = array();
if ($conf->posted) {
	//autogenerate database key
	$conf->DBpassword = generateKey(16);
	
	if( $conf->Sitename == "" ) {
		$errs["Sitename"] = wfMsg('Page.Install.error-blank-sitename');
	}
	if( strlen($conf->Sitename) > $wgSitenameLength ) {
		$errs["Sitename"] = wfMsg('Page.Install.error-sitename-exceeds-max-length');
	}
	if( $conf->DBuser == "" ) {
		$errs["DBuser"] = wfMsg('Page.Install.error-blank-db-username');
	}
	if( $conf->SysopName == "" ) {
		$errs["SysopName"] = wfMsg('Page.Install.error-blank-username');
	}
	if( strcasecmp($conf->SysopName, 'anonymous') == 0 ) {
		$conf->SysopName = 'Admin';
	}
	if( $conf->SysopEmail == "" ) {
		$errs["SysopEmail"] = wfMsg('Page.Install.error-blank-useremail');
	}
	if( ($conf->DBtype == 'mysql') && (strlen($conf->DBuser) > 16) ) {
		$errs["DBuser"] = wfMsg('Page.Install.error-db-usernamelong');
	}
	if ($conf->DBtype != 'mysql') {
		$errs["DBtype"] = wfMsg('Page.Install.error-db-support');
	}
	
	if ($wgIsEnterprise) 
	{
		if ($conf->RegistrarFirstName == '') {
			$errs['RegistrarFirstName'] = wfMsg('Page.Install.error-noname');
		}
		if ($conf->RegistrarLastName == '') {
			$errs['RegistrarLastName'] = wfMsg('Page.Install.error-noname');
		}
		if ($conf->RegistrarPhone == '') {
			$errs['RegistrarPhone'] = wfMsg('Page.Install.error-nophone');
		}
		if ($conf->RegistrarCount == '0') {
			$errs['RegistrarCount'] = wfMsg('Page.Install.error-registrarcount');
		}
	}
}

error_reporting( E_ALL );

/**
 * Validate the initial administrator account; username,
 * password checks, etc.
 */
if( $conf->posted && $conf->SysopName ) {
	# Check that the user can be created
	# Various password checks
	if( $conf->SysopPass != '' ) {
		if( $conf->SysopPass != $conf->SysopPass2 ) {
			$errs['SysopPass2'] = wfMsg('Page.Install.error-password-match');
		}
	} else {
		$errs['SysopPass'] = wfMsg('Page.Install.error-blank-password');
	}
}

if( $conf->posted && ( 0 == count( $errs ) ) ) {
	print('<div id="install-output" class="env-check">');
	print('<fieldset><legend>'.wfMsg('Page.Install.ui-installing').'</legend>');
	print('<ul class="env-check">');
	
	/* So we can 'continue' to end prematurely */
	do 
	{ 
		$conf->Root = ($conf->RootPW != "");

		//generate the PHP and XML for settings that are generated
		$localsettings = install_localsettings_generate( $conf );
		$startupxml = install_mindtouch_xml_generate( $conf );

		$wgCommandLineMode = false;
		
		//verify LocalSettings.php is OK
		chdir( ".." );
		$oklocal = eval( $localsettings );
		if( $oklocal === false ) 
		{
			echo_fail(wfMsg('Page.Install.error-fatal-bug', $localsettings)); 
			//todo: how to recover?
			break;
		}
		
		$conf->DBtypename = '';
		foreach (array_keys($ourdb) as $db) 
		{
			if ($conf->DBtype === $db)
				$conf->DBtypename = $ourdb[$db]['fullname'];
		}
		if ( ! strlen($conf->DBtype)) 
		{
			$errs["DBpicktype"] = wfMsg('Page.Install.error-db-type');
			continue;
		}

		if (! $conf->DBtypename) 
		{
			$errs["DBtype"] = wfMsg('page.Install.error-db-unknown', $conf->DBtype);
			continue;
		}
		
		//initialize Setup.php
		$wgDBtype = $conf->DBtype;
		$wgCommandLineMode = true;
		require_once( "includes/Setup.php" );
		chdir( "config" );

		//do the database install
		error_reporting( E_ALL );
		$db_install = install_database($conf, $useRoot);
		
		//if database install went well, continue onwards
		if ($db_install) 
		{
			//Write the settings we have for LocalSettings.php to disk
			install_write_settings_file(FILE_LOCALSETTINGS, install_php_wrapper($localsettings));
	
			//Write the settings we have for mindtouch.deki.startup.xml to disk
			install_write_settings_file(FILE_STARTUP_XML, install_mindtouch_xml_generate($conf));
	
			//Write the settings we have for mindtouch.host.conf (or mindtouch.host.bat) to disk
			$confOutputFile = wfIsWindows() ? FILE_HOST_WIN_BAT: FILE_HOST_XML;
			install_write_settings_file($confOutputFile, install_mindtouch_conf_generate($conf));
			
			// retrieve the license information
			if ($wgIsEnterprise) 
			{
				global $wgProductTrialUrl;
				echo('<li>'.wfMsg('Page.Install.license.retrieve')); flush();
				$Plug = new Plug($wgProductTrialUrl, null);
				$Result = $Plug
					->With('name', $conf->RegistrarFirstName . ' ' . $conf->RegistrarLastName)
					->With('email', $conf->SysopEmail)
					->With('phone', $conf->RegistrarPhone)
					->With('productkey', md5($conf->ApiKey))
					->With('product', $wgCommercialType)
					->With('from', 'installer')
					->Post();
				$licensePost = $Result['status'] == 200;
				if ($licensePost) {
					echo(' '.wfMsg('Page.Install.completed').'</li>');
					flush();
				}
				else 
				{
					echo(' <span style="color:red;">'.wfMsg('Page.Install.license.failed').'</span></li>');
					flush();	
				}
			}
				
			//magical work for VM since we know the server environment
			//todo: consolidate this with the output for other OSes
			if ($wgIsVM) 
			{
				// copy the appropriate files and start dekihost
				rename(FILE_LOCALSETTINGS, $IP.'/'.FILE_LOCALSETTINGS);
				rename(FILE_STARTUP_XML, '/etc/dekiwiki/'.FILE_STARTUP_XML);
				rename($confOutputFile, '/etc/dekiwiki/'.$confOutputFile);
				echo('<li>'.wfMsg('Page.Install.dekihost.start'));
				flush();
				exec("sudo /etc/init.d/dekiwiki restart > /dev/null 2>&1");
                sleep(2); //let dekihost start up
				echo(' '.wfMsg('Page.Install.completed').'</li>');
				flush();
			}
			
			if ($wgIsMSI) 
			{
				// the MSI will work by giving write access to specific conf files, and targeting the files for writing
				wfSetFileContent($IP.'/'.FILE_LOCALSETTINGS, wfGetFileContent(FILE_LOCALSETTINGS));
				wfSetFileContent($wgPathConf.'/'.FILE_STARTUP_XML, wfGetFileContent(FILE_STARTUP_XML));
			}
		}
		print("</ul>");
		echo('</fieldset>');
		
		if ($db_install) 
		{
			writeSuccessMessage($conf->ApiKey, $conf->SysopPass, $conf->getSysopName());
			finalize_product_installation($conf);
		}
	} while( false );
}
else {
	initialize_product_installation($conf);
}
?>
</ul>

<?php
if ( count( $errs ) ) {
	if( $conf->posted ) {
		echo '<div class="newerror">'.wfMsg('Page.Install.ui-errors').'</div>';
	}
	echo('<ul>');	
	//display errors if they happened on install
	foreach ($errs as $err) 
	{
		echo_fail($err);
	}
	echo('</ul>');
}

if( !$conf->posted || count( $errs ) > 0 || ($conf->posted && !$db_install) ) {
	include('skin.form.php');
}

include('skin.footer.php');
?>
