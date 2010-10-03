<?php

function install_php_wrapper( $phpContents ) 
{
	$phpFile = "<" . "?php" . NEWLINE . $phpContents . "?" . ">";
	$phpFile = str_replace( "\r\n", "\n", $phpFile );
	return $phpFile;
}


function escapePhpString( $string ) {
	return strtr( $string,
		array(
			"\n" => "\\n",
			"\r" => "\\r",
			"\t" => "\\t",
			"\\" => "\\\\",
			"\$" => "\\\$",
			"\"" => "\\\""
		));
}

class ConfigData 
{
	function getEncoded( $data ) 
	{
		# removing latin1 support, no need...
		return $data;
	}
	function getSysopName() { return $this->getEncoded( $this->SysopName ); }
	function getSysopEmail() { return $this->getEncoded( $this->SysopEmail ); }
	function getSysopPass() { return $this->getEncoded( $this->SysopPass ); }

	function setSchema( $schema ) 
	{
		$this->DBschema = $schema;
		switch ( $this->DBschema ) {
			case 'mysql5':
				$this->DBTableOptions = 'ENGINE=InnoDB, DEFAULT CHARSET=utf8';
				$this->DBmysql5 = 'true';
				break;
			case 'mysql5-binary':
				$this->DBTableOptions = 'ENGINE=InnoDB, DEFAULT CHARSET=binary';
				$this->DBmysql5 = 'true';
				break;
			default:
				$this->DBTableOptions = 'TYPE=InnoDB';
				$this->DBmysql5 = 'false';
		}
		# Set the global for use during install
		global $wgDBTableOptions;
		$wgDBTableOptions = $this->DBTableOptions;
	}
}


function dieout( $text ) 
{
	die( $text . "\n\n</body>\n</html>" );
}

function echo_warn() 
{
	echo(formatmessage(func_get_args(), 'warn'));
}
function echo_list() 
{
	echo(formatmessage(func_get_args(), ''));
}
function echo_success() 
{
	echo(formatmessage(func_get_args(), 'success'));
}
function echo_fail() 
{
	echo(formatmessage(func_get_args(), 'error'));
}
function formatmessage( $args, $type ) 
{
	$list = sprintf('<li class="%s"><strong class="%s">%s</strong>: ', $type, $type, wfMsg('Page.Install.type-'.$type));
	echo(sprintf($list.'%s', count($args) > 1 ? call_user_func_array('sprintf', $args): $args[0]));
}


function importVar( &$var, $name, $default = "" ) 
{
	if( isset( $var[$name] ) ) {
		$retval = $var[$name];
		if ( get_magic_quotes_gpc() ) {
			$retval = stripslashes( $retval );
		}
	} else {
		$retval = $default;
	}
	return $retval;
}

function importPost( $name, $default = "" ) 
{
	return importVar( $_POST, $name, $default );
}

function importCheck( $name, $default = false ) 
{
	return isset( $_POST[$name] ) ? true: $default;
}

function importRequest( $name, $default = "" ) 
{
	return importVar( $_REQUEST, $name, $default );
}

function generateKey( $length = 32 ) 
{
	$pattern = "0123456789aAbBcCdDeEfFgGhHiIjJkKlLmMnNoOpPqQrRsStTuUvVwWxXyYzZ";
	$key = "";
	for($i = 0; $i < $length; $i++) {
		$key .= $pattern{mt_rand(0,strlen($pattern)-1)};
	}
	return $key;
}

//visual helpers

$radioCount = 0;

function aField( &$conf, $field, $text, $type = "text", $value = "", $onclick = '' ) {
	global $radioCount;
	if( $type != "" ) {
		$xtype = "type=\"$type\"";
	} else {
		$xtype = "";
	}

	$id = $field;
	$nolabel = ($type == "radio") || ($type == "hidden");

	if ($type == 'radio')
		$id .= $radioCount++;

	if( $nolabel ) {
		echo "\t\t<label>";
	} else {
		echo "\t\t<label class='column' for=\"$id\">$text</label>\n";
	}

	if( $type == "radio" && $value == $conf->$field ) {
		$checked = "checked='checked'";
	} else {
		$checked = "";
	}
	echo "\t\t<input $xtype name=\"$field\" id=\"$id\" class=\"iput-$type\" $checked ";
	if ($onclick) {
		echo " onclick='toggleDBarea(\"$value\",1)' " ;
	}
	echo "value=\"";
	if( $type == "radio" ) {
		echo htmlspecialchars( $value );
	} else {
		echo htmlspecialchars( $conf->$field );
	}


	echo "\" />\n";
	if( $nolabel ) {
		echo " $text</label>\n";
	}

	global $errs;
	if(isset($errs[$field])) echo "<span class='error'>" . $errs[$field] . "</span>\n";
}

function database_picker($conf) {
	global $ourdb;
	print "\n";
	foreach(array_keys($ourdb) as $db) {
		if ($ourdb[$db]['havedriver']) {
			print "<li>";
			aField( $conf, "DBtype", $ourdb[$db]['fullname'], 'radio', $db, 'onclick');
			print "</li>\n";
		}
	}
	print "\n";
}
/**
 * Method called on first page load with no post state.
 *
 * @param Object $conf
 */
function initialize_product_installation($conf)
{
	$scheme = isset($_SERVER['HTTPS']) && $_SERVER['HTTPS'] == 'on' ? 'https' : 'http';

	echo '<div style="position:absolute; top:-50px; left:-50px;">';
		echo '<iframe width="1" height="1" src="'.$scheme.'://assets.mindtouch.com/install/?modtime='.time().'"></iframe>';
	echo '</div>';
	
}

/**
 * Method called after installation completes successfully. Can generate markup.
 *
 * @param Object $conf
 * @return
 */
function finalize_product_installation($conf)
{
	$scheme = isset($_SERVER['HTTPS']) && $_SERVER['HTTPS'] == 'on' ? 'https' : 'http';

	$keys = generate_post_keys($conf);
	// add a partial-hash to verify against
	$keys['hash'] = substr(md5(implode(',', $keys)), 0, 5);
	// generate the query string
	$query = http_build_query($keys);
	
	// this'll help us see what % of users make it through installation
	echo '<div style="position:absolute; top:-50px; left:-50px;">';
		echo '<iframe width="1" height="1" src="'.$scheme.'://assets.mindtouch.com/install/?'. $query .'"></iframe>';
	echo '</div>';
}

/**
 * Retrieve a string identifying the current install method
 * 
 * @param bool $normalize - if true, the method will be lowercased with spaces => dashes
 * @return string
 */
function get_installation_method($normalize = false)
{
	global $wgIsVM, $wgIsAMI, $wgIsLinuxPkg, $wgIsEsxVM, $wgIsMSI;
	
	// what is the install platform?
	$installMethod = '';
	if (isset($wgIsVM) && $wgIsVM)
	{
		$installMethod = 'VM';
	}
	else if (isset($wgIsAMI) && $wgIsAMI)
	{
		$installMethod = 'AMI';
	}
	else if (isset($wgIsLinuxPkg) && $wgIsLinuxPkg)
	{
		$installMethod = 'Package';
	}
	else if (isset($wgIsEsxVM) && $wgIsEsxVM)
	{
		$installMethod = 'VM ESX';
	}
	else if (isset($wgIsMSI) && $wgIsMSI)
	{
		$installMethod = 'MSI';
	}
	else 
	{
		$installMethod = 'Source';
	}
	
	return $normalize ? strtolower(str_replace(' ', '-', $installMethod)) : $installMethod;
}

function generate_post_keys($conf)
{
	global $wgIsEnterprise;
	
	// what type of install is it?
	$installType = 'Core';
	if ($wgIsEnterprise)
	{
		$installType = 'Trial';
	}
	
	/**
	 * Generate the array of post keys
	 */
	$keys = array(
		// note: keep important keys at the top
		'first_name' => $conf->RegistrarFirstName,
		'last_name' => $conf->RegistrarLastName,
		'email' => $conf->SysopEmail,
		'company' => $conf->Sitename,
		'phone' => $conf->RegistrarPhone,
		'lead_source' => 'install',
		'install_method' => get_installation_method(),
		'install_type' => $installType,
		'country' => substr($conf->SiteLang, 3, 2),
		'os' => PHP_OS,
		'department' => $conf->RegistrarDept,
		'size' => $conf->RegistrarCount, 
		'usage' => $conf->RegistrarUsage,
	);
	
	return $keys;
}

?>
