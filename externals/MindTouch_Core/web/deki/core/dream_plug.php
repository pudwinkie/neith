<?php
/*
 * MindTouch Deki - enterprise collaboration and integration platform
 * Copyright (C) 2006-2009 MindTouch, Inc.
 * www.mindtouch.com  oss@mindtouch.com
 *
 * For community documentation and downloads visit www.opengarden.org;
 *  please review the licensing section.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along
 * with this program; if not, write to the Free Software Foundation, Inc.,
 * 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.
 * http://www.gnu.org/copyleft/gpl.html
 */

/**
 * DreamPlug
 * Base Plug class for interfacing with a Dream service
 */
class DreamPlug
{
	/**
	 * Determines which headers should be forwarded with every request
	 * 
	 * @var array
	 */
	static $defaultHeaders = array(
		'X-Forwarded-For' => 'HTTP_X_FORWARDED_FOR',
		'X-Forwarded-Host' => 'HTTP_HOST',
		'Referer' => 'HTTP_REFERER',
		'User-Agent' => 'HTTP_USER_AGENT'
	);

	// Http answers
	const HTTPSUCCESS = 200;
	const HTTPNOTFOUND = 404;
	const HTTPAUTHFAILED = 401;
	
	/**
	 * String $classname - determines the class to instantiate, allows plug to be extended
	 */
	protected $classname = null;
	/**
	 * @access protected
	 */
	protected $scheme;
	protected $user;
	protected $password;
	protected $host;
	// guerrics: hack to store the dream.in.host value
	protected $hostname;
	protected $port;
	protected $path;
	protected $query;
	protected $fragment;
	protected $timeout = 300;
	protected $headers = array();

	/**
	 * Dream specific urlencode method
	 * @see Bugfix#7500: Unable to save a new page with a dot (.) at the end of the title on IIS
	 * 
	 * @param string $string - string to urlencode
	 * @param bool $doubleEncode - if true, the string will be urlencoded twice
	 * @return string
	 */
	public static function urlencode($string, $doubleEncode = false)
	{
		// encode trailing dots (. => %2E)
		for ($i = strlen($string) - 1, $dots = 0; $i >= 0; $dots++, $i--)
		{
			if (substr($string, $i, 1) != '.')
			{
				break;
			}
		}
		$string = urlencode(substr($string, 0, $i + 1)) . str_repeat('%2E', $dots);
		
		// we don't need to apply our custom encodings on the second pass
		if ($doubleEncode)
		{
			$string = urlencode($string);
		}
		
		return $string;
	}

	/**
	 * Constructor
	 *
	 * @param mixed $uri
	 * @param string $output, optional, default = 'php'
	 * @param string $hostname, optional, default = null
	 */
	public function __construct($uri = null, $output = 'php', $hostname = null) 
	{
		// set the actual classname
		$this->classname = get_class($this);

		// initialize from uri string
		if (is_string($uri))
		{
			$uri = parse_url($uri);
		}
		
		$this->headers = array();

		// initialize from Plug object
		if (is_object($uri))
		{
			$this->scheme = $uri->scheme;
			$this->user = $uri->user;
			$this->password = $uri->password;
			$this->host = $uri->host;
			$this->hostname = $uri->hostname;
			$this->port = $uri->port;
			$this->path = $uri->path;
			$this->query = $uri->query;
			$this->fragment = $uri->fragment;
			$this->timeout = $uri->timeout;
			$this->headers = $uri->headers;
		}

		// initialize from uri array
		if (is_array($uri))
		{
			$this->scheme = isset($uri['scheme']) ? $uri['scheme'] : null;
			$this->user = isset($uri['user']) ? $uri['user'] : null;
			$this->password = isset($uri['pass']) ? $uri['pass'] : null;
			$this->host = isset($uri['host']) ? $uri['host'] : null;
			$this->port = isset($uri['port']) ? $uri['port'] : null;
			$this->path = isset($uri['path']) ? $uri['path'] : null;
			$this->query = isset($uri['query']) ? $uri['query'] : null;
			$this->fragment = isset($uri['fragment']) ? $uri['fragment'] : null;
		}

		// set php output option
		// bugfix 3445: Plug does not append dream.in.* values when PHP output is not set
		// when you invoke plug w/o the PHP output, we need these values appended automatically.
		// To prevent multiple values from being appended after calling At(),  With(), etc check $output != false.
		if ($output != false)
		{
			if($this->query) {
				$this->query .= '&';
			} else {
				$this->query = '';
			}
			
			if (empty($hostname))
			{
				$hostname = isset($_SERVER['HTTP_HOST']) ? $_SERVER['HTTP_HOST'] : '';
			}
			
			$this->query .= 'dream.out.format=' . rawurlencode($output);
			// if a hostname was previously set, reuse it, otherwise take the new one
			$this->query .= '&dream.in.host=' . rawurlencode(!empty($this->hostname) ? $this->hostname : $hostname);

			//hack hack, pass in scheme until dream.in.uri is available
			// parse the scheme from the frontend request
			if(isset($_SERVER['HTTPS']) &&  $_SERVER['HTTPS'] == "on")
				$scheme = 'https';
			else
				$scheme = 'http';

			$this->query .= '&dream.in.scheme=' . $scheme;
			
			if (isset($_SERVER['REMOTE_ADDR'])) {
				$this->query .= '&dream.in.origin=' . rawurlencode($_SERVER['REMOTE_ADDR']);
			}
		}
		
		// included default & white-listed headers
		if (empty($this->headers))
		{
			$this->setDefaultHeaders();
		}
	}

	/**
	 * Returns a list of the headers that have been set
	 * 
	 * @return mixed - array if there are headers. bool false if there are no headers.
	 */
	public function GetHeaders()
	{
		$headers = array();
		if (is_array($this->headers) && sizeof($this->headers) > 0)
		{
			foreach ($this->headers as $name => $value)
			{
				$headers[] = $name .': '. $value;
			}
			return $headers;
		}
		else
		{
			return false;
		}
	}

	/**
	 * Retrieves the fully generate uri
	 * 
	 * @return string - uri
	 */
	public function GetUri()
	{
		$uri = $this->scheme ? $this->scheme . ':' . ((strtolower($this->scheme) == 'mailto') ? '' : '//') : '';
		//$uri .= $this->user ?  $this->user . ( $this->password ? ':' .  $this->password : '' ) . '@' : '';
		$uri .= $this->host ? $this->host : '';
		$uri .= $this->port ? ':' . $this->port : '';
		$uri .= $this->path ? $this->path : '';
		$uri .= $this->query ? '?'. $this->query : '';
		$uri .= $this->fragment ? '#' . $this->fragment : '';
		return $uri;
	}

	/**
	 * Uri builder
	 * 
	 * @param $path[] - method takes any number of path components
	 * @return DreamPlug
	 */
	public function At(/* $path[] */)
	{
		$result = new $this->classname($this, false);

		foreach(func_get_args() as $path)
		{
			$result->path .= '/' . $path;
		}
		return $result;
	}

	/**
	 * Appends to the query string GET variables
	 * @TODO: we should not write directly to string here; we should operate on an array and convert the array when we POST; 
	 * this'll allow us to dynamic transformations of URLs
	 * 
	 * @param string $name - variable name
	 * @param string $value - variable value
	 * @return DreamPlug
	 */
	public function With($name, $value = false)
	{
		$result = new $this->classname($this, false);

		if ($result->query)
		{
			$result->query .= '&' . urlencode($name) . ($value !== false ? '=' . urlencode($value) : '');
		}
		else
		{
			$result->query = urlencode($name) . ($value !== false ? '=' . urlencode($value) : '');
		}

		return $result;
	}

	/**
	 * Add header value to pass with the request
	 * 
	 * @param $name - header name
	 * @param $value - header value
	 * @return DreamPlug
	 */
	public function WithHeader($name, $value)
	{
		$result = new $this->classname($this, false);
		$result->headers[$name] = $value;

		return $result;
	}

	/**
	 * @deprecated Use WithHeader which is symmetric with C# Plug
	 */
	public function SetHeader($name, $value)
	{
		return $this->WithHeader($name, $value);
	}

	/**
	 * Adds standard HTTP auth credentials for the request
	 * 
	 * @param string $user - user name to use for authorization
	 * @param string $password
	 * @return DreamPlug
	 */
	public function WithCredentials($user, $password)
	{
		$result = new $this->classname($this, false);
		$result->user= $user;
		$result->password = $password;

		return $result;
	}

	/**
	 * Performs a GET request
	 * 
	 * @return mixed - request response
	 */
	public function Get()
	{
		return $this->Invoke('GET', null, false);
	}

	public function GetMessage()
	{
		return $this->Invoke('GET');
	}

	/**
	 * Performs a HEAD request
	 * 
	 * @return mixed - request response
	 */
	public function Head()
	{
		return $this->Invoke('HEAD', null, false);
	}

	/**
	 * Performs a POST request
	 * 
	 * @param mixed $input - if array, gets encoded as xml. otherwise treated at post fields.
	 * @return mixed - request response
	 */
	public function Post($input = null)
	{
		if (is_array($input))
		{
			return $this->InvokeXml('POST', $input);
		}
		else
		{
			return $this->InvokeFields('POST', $input);
		}
	}	

	public function PostMessage($xml = null)
	{
		return $this->InvokeXml('POST', $xml);
	}	

	public function PostFields($formFields)
	{
		return $this->InvokeFields('POST', $formFields);
	}
	
	/**
	 * Performs a PUT request
	 * 
	 * @param array $xml
	 * @return mixed - request response
	 */	
	public function Put($xml = null)
	{
		$r = $this->With('dream.in.verb', 'PUT');
		return $r->InvokeXml('POST', $xml);
	}
	
	public function PutFields($formFields)
	{
		return $this->InvokeFields('PUT', $formFields);
	}
	
	public function PutFile($content = array())
	{
		return $this->Invoke('PUT', $content, false);
	}	

	/**
	 * Performs a DELETE request
	 * 
	 * @param array $xml
	 * @return mixed - request response
	 */	
	public function Delete($xml = null)
	{
		//Mono has a Content-length: 0 bug, so we can use Dream's faux-method handling
		$r = $this->With('dream.in.verb', 'DELETE');
		return $r->InvokeXml('POST', $xml);
	}
	
	/**
	 * Invokes xml
	 * 
	 * @param $verb
	 * @param $xml
	 * @return array
	 */
	protected function InvokeXml($verb, $xml)
	{
		if (is_array($xml))
		{
			$xml = encode_xml($xml);
		}

		// guerrics: adding empty check since dream dies on empty xml bodies
		if (!empty($xml) && empty($this->headers['Content-Type']))
		{
			$this->headers['Content-Type'] = 'application/xml';
		}

		return $this->Invoke($verb, $xml, false);
	}
	
	/**
	 * Invokes form fields
	 * 
	 * @param $verb
	 * @param $formFields
	 * @return array
	 */
	protected function InvokeFields($verb, $formFields)
	{
		return $this->Invoke($verb, $formFields);
	}

	/**
	 * Invokes the request
	 * 
	 * @param $verb
	 * @param $content
	 * @param $callback
	 * @return array
	 */
	protected function Invoke($verb, $content = null, $callback = false)
	{
		$uri = $this->GetUri();

		// prepare request
		$curl = curl_init();
		curl_setopt($curl, CURLOPT_URL, $uri);
		curl_setopt($curl, CURLOPT_RETURNTRANSFER, 1);
		// proxy configuration
		//curl_setopt($curl, CURLOPT_PROXY, 'X.X.X.X:8888');
		curl_setopt($curl, CURLOPT_TIMEOUT, $this->timeout);
		curl_setopt($curl, CURLOPT_FOLLOWLOCATION, 1);
		curl_setopt($curl, CURLOPT_MAXREDIRS, 10);
		curl_setopt($curl, CURLOPT_CUSTOMREQUEST, $verb);
		curl_setopt($curl, CURLOPT_SSL_VERIFYPEER, false);

		// empty() will mean that string "0" will be set to 0 content-length; !$content is too fuzzy
		if (is_null($content) || $content === false || (is_string($content) && strlen($content) == 0))
		{
			$this->headers['Content-Length'] = 0;
		}
		else
		{
			// remove any set content-length
			// useful when performing a Get request followed by a POST/PUT
			$contentLength = isset($this->headers['Content-Length']) ? $this->headers['Content-Length'] : null;
			if ($contentLength === 0)
			{
				unset($this->headers['Content-Length']);
			}
		}

		$this->ApplyCredentials($curl);
		if ($verb == 'PUT')
		{
			curl_setopt($curl, CURLOPT_PUT, true);
			if (isset($content['file_temp']))
			{
				if (is_file($content['file_temp']))
				{
					curl_setopt($curl, CURLOPT_INFILE, fopen($content['file_temp'], 'r'));
					curl_setopt($curl, CURLOPT_INFILESIZE, filesize($content['file_temp']));
					$this->headers["Content-Type"] =  $content['file_type'];
				}

				// remove the PutFile content
				$content = array();
			}
		}
		else if ($verb == 'POST')
		{
			curl_setopt($curl, CURLOPT_POSTFIELDS, $content);
		}
		
		// add the request headers
		if (!empty($this->headers))
		{
			curl_setopt($curl, CURLOPT_HTTPHEADER, $this->GetHeaders());
		}
		// retrieve the response headers
		curl_setopt($curl, CURLOPT_HEADER, true);
		
		// execute request
		$result = array();
		$this->preExecuteRequest($curl, $verb, $content, $callback);
		$response = curl_exec($curl);
		$this->postExecuteRequest($curl, $verb, $content, $response);
		
		$status = curl_getinfo($curl, CURLINFO_HTTP_CODE);
		$type = curl_getinfo($curl, CURLINFO_CONTENT_TYPE);
		$result['errno'] = curl_errno($curl);
		$result['error'] = curl_error($curl);
		curl_close($curl);
		
		// header parsing
		$result['headers'] = array();
		// make sure ther response is not empty before trying to parse
		// also make sure there isn't a curl error
		if ($status != 0 && $result['errno'] == 0)
		{
			// split response into header and response body
			do
			{
				list($headers, $response) = explode("\r\n\r\n", $response, 2);
				$headers = explode("\r\n", $headers);
				// First line of headers is the HTTP response code
				$httpStatus = array_shift($headers); // remove!
				// check if there is another header chunk to parse
			} while ($httpStatus == 'HTTP/1.1 100 Continue');

			// put the rest of the headers in an array
			foreach ($headers as $headerLine)
			{
				list($header, $value) = explode(': ', $headerLine, 2);
				//$result['headers'][$header] .= $value."\n";
				// don't acknowledge multiple headers
				$result['headers'][$header] = trim($value);
			}
			// /header parsing
		}
		
		// check if we need to deserialize
		if (strpos($type, '/php'))
		{
			$response = unserialize($response);
		}
		
		$result['request'] = array('uri' => $this->getUri(), 'body' => $content);
		$result['uri'] = $uri;
		$result['body'] = $response;
		$result['status'] = $status;
		$result['type'] = $type;
		
		// check if we need to invoke a callback
		if ($callback)
		{
			return call_user_func_array($callback, array(&$result));
		}
		else
		{
			return $result;
		}
	}
	
	/**
	 * Applys credentials for the request
	 * 
	 * @param $curl
	 * @return
	 */
	protected function ApplyCredentials($curl)
	{
		// apply manually given credentials
		if (isset($this->user) || isset($this->password))
		{
			$this->headers['Authorization'] = 'Basic ' . base64_encode($this->user . ':' . $this->password);
		}
		else if (function_exists("getallheaders"))
		{
			$headers = getallheaders();
		
			if (isset($headers['Authorization']))
			{
				// Use encoded credentials from the php request header. (e.g. Basic c3lzb3A6c3lzb3A=)
				$this->headers['Authorization'] = $headers['Authorization'];
			}
		}
	}

	/**
	 * Method configures Plug with the default headers and forwarded headers
	 * @return
	 */
	protected function setDefaultHeaders()
	{
		foreach (self::$defaultHeaders as $header => $key)
		{
			if (isset($_SERVER[$key]))
			{
				$this->headers[$header] = $_SERVER[$key];
			}
		}

		// append REMOTE_ADDR to X-Forwarded-For if it exists
		if (isset($_SERVER['REMOTE_ADDR']))
		{
			$this->headers['X-Forwarded-For'] = isset($this->headers['X-Forwarded-For'])
				? $this->headers['X-Forwarded-For'].', '.$_SERVER['REMOTE_ADDR']
				: $_SERVER['REMOTE_ADDR']
			;
		}
	}

	/**
	 * @stub
	* @note Method can be extended to modify the curl request before sending
	 * @param $curl
	 * @param $verb
	 * @param $content
	 * @return
	 */
	protected function preExecuteRequest(&$curl, &$verb, &$content, &$callback) {}

	/**
	 * @stub
	 * @param $curl
	 * @param $verb
	 * @param $content
	 * @param $response
	 * @return
	 */
	protected function postExecuteRequest(&$curl, &$verb, &$content, &$response) {}
}

/*
 * Helper functions
 */
if (!function_exists('encode_xml')) :
/**
 * Helper for sending an HTTP status header
 */
function http_status($num)
{
	static $http = array(
		100 => "HTTP/1.1 100 Continue",
		101 => "HTTP/1.1 101 Switching Protocols",
		200 => "HTTP/1.1 200 OK",
		201 => "HTTP/1.1 201 Created",
		202 => "HTTP/1.1 202 Accepted",
		203 => "HTTP/1.1 203 Non-Authoritative Information",
		204 => "HTTP/1.1 204 No Content",
		205 => "HTTP/1.1 205 Reset Content",
		206 => "HTTP/1.1 206 Partial Content",
		300 => "HTTP/1.1 300 Multiple Choices",
		301 => "HTTP/1.1 301 Moved Permanently",
		302 => "HTTP/1.1 302 Found",
		303 => "HTTP/1.1 303 See Other",
		304 => "HTTP/1.1 304 Not Modified",
		305 => "HTTP/1.1 305 Use Proxy",
		307 => "HTTP/1.1 307 Temporary Redirect",
		400 => "HTTP/1.1 400 Bad Request",
		401 => "HTTP/1.1 401 Unauthorized",
		402 => "HTTP/1.1 402 Payment Required",
		403 => "HTTP/1.1 403 Forbidden",
		404 => "HTTP/1.1 404 Not Found",
		405 => "HTTP/1.1 405 Method Not Allowed",
		406 => "HTTP/1.1 406 Not Acceptable",
		407 => "HTTP/1.1 407 Proxy Authentication Required",
		408 => "HTTP/1.1 408 Request Time-out",
		409 => "HTTP/1.1 409 Conflict",
		410 => "HTTP/1.1 410 Gone",
		411 => "HTTP/1.1 411 Length Required",
		412 => "HTTP/1.1 412 Precondition Failed",
		413 => "HTTP/1.1 413 Request Entity Too Large",
		414 => "HTTP/1.1 414 Request-URI Too Large",
		415 => "HTTP/1.1 415 Unsupported Media Type",
		416 => "HTTP/1.1 416 Requested range not satisfiable",
		417 => "HTTP/1.1 417 Expectation Failed",
		500 => "HTTP/1.1 500 Internal Server Error",
		501 => "HTTP/1.1 501 Not Implemented",
		502 => "HTTP/1.1 502 Bad Gateway",
		503 => "HTTP/1.1 503 Service Unavailable",
		504 => "HTTP/1.1 504 Gateway Time-out"
	);

	header($http[$num]);
}

function is_numeric_array($data)
{
	return is_array($data) && (count($data) > 0) && isset($data[0]);
}

/**
 * Helper method for encoding xml
 */
function encode_xml($data, $outer = null)
{
	$result = '';
	if(is_array($data)) {
		foreach($data as $key => $value) {
			if(strncmp($key, '@', 1) == 0) {

				// skip attributes
			} else {
				$tag = $outer ? $outer : $key;
				if(is_numeric_array($value)) {
					$result .= encode_xml($value, $key);
				} elseif(is_array($value)) {
					$attrs = '';
					foreach($value as $attr_key => $attr_value) {
						if(strncmp($attr_key, '@', 1) == 0) {
							$attrs .= ' ' . substr($attr_key, 1) . '="' . htmlspecialchars($attr_value) . '"';
						}
					}
					$result .= '<' . $tag . $attrs . '>' . encode_xml($value) . '</' . $tag . '>';
				} elseif($tag != '#text') {
					$result .= '<' . $tag . '>' . encode_xml($value) . '</' . $tag . '>';
				} else {
					$result .= htmlspecialchars($value);
				}
			}
		}
	} elseif(is_string($data)) {
		return htmlspecialchars($data);
	} else {
	
		// TODO (steveb): how we should handle this case?
		$result = $data;
	}
	return $result;
}

endif;
