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

// used for api profiling
// @TODO guerrics: better stat capture for profiling
global $wgPlugProfile;
if (!isset($wgPlugProfile) || !is_array($wgPlugProfile))
{
	$wgPlugProfile = array();
}

/**
 * DekiPlug
 * Handles authentication and additional functionality when
 * interacting with the MindTouch REST API.
 */
class DekiPlug extends DreamPlug
{
	// @var DekiPlug - stores the Plug for the current request
	static $instance = null;

	// used for calculating request profiling information
	private $requestTimeStart = null;
	private $requestTimeEnd = null;
	private $requestVerb = null;

	/**
	 * Retrieve the DekiPlug object that was set for this request
	 * 
	 * @return DekiPlug
	 */
	static function &getInstance()
	{
		if (!is_object(self::$instance))
		{
			throw new Exception('DekiPlug has not be initialized yet.');
		}

		return self::$instance;
	}
	
	/**
	 * Set the plug object for this request
	 * 
	 * @param DekiPlug $Plug
	 * @return
	 */
	static function setInstance($Plug)
	{
		self::$instance = $Plug;
	}
	
	/**
	 * Method retrieves the apikey
	 * @note used in the control panel with an auxillary config
	 * @TODO guerrics: use a config class to retrieve the key for site/settings
	 */
	static function getApiKey()
	{
		global $wgDekiApiKey;

		return isset($wgDekiApiKey) && !empty($wgDekiApiKey) ? $wgDekiApiKey : Config::$API_KEY;
	}

	/**
	 * DekiPlug constructor
	 * @see DreamPlug#__construct
	 */

	/**
	 * @see DreamPlug#Delete()
	 * 
	 * @return DekiResult
	 */
	public function Delete($input = null)
	{
		$result = parent::Delete($input);
		return new DekiResult($result);
	}
	
	/**
	 * @see DreamPlug#Post()
	 * 
	 * @return DekiResult
	 */
	public function Post($input = null)
	{
		$result = parent::Post($input);
		return new DekiResult($result);
	}
	
	/**
	 * @see DreamPlug#Put()
	 * 
	 * @return DekiResult
	 */
	public function Put($input = null)
	{
		$result = parent::Put($input);
		return new DekiResult($result);
	}

	/**
	 * Puts a file from the filesystem
	 * 
	 * @param $path - location of the file on the filesystem
	 * @param $mimeType - mimetype of the file being uploaded
	 * @return DekiResult
	 */
	public function PutFile($path, $mimeType = null)
	{
		$result = parent::PutFile(array(
			'file_temp' => $path,
			'file_type' => $mimeType
		));

		return new DekiResult($result);
	}

	/**
	 * @see DreamPlug#Get()
	 * 
	 * @return DekiResult
	 */
	public function Get()
	{
		$result = parent::Get();
		return new DekiResult($result);
	}
	
	/**
	 * @see DreamPlug#Head()
	 * 
	 * @return DekiResult
	 */
	public function Head()
	{
		$result = parent::Head();
		return new DekiResult($result);
	}

	/**
	 * The api requires double urlencoded titles. This method will do it automatically for you.
	 * @see #AtRaw() for creating unencoded path components
	 * 
	 * @param string[] $path - path components to add to the request 
	 * @return DekiPlug
	 */
	public function At(/* $path[] */) 
	{
		$result = new $this->classname($this, false);

		foreach (func_get_args() as $path) 
		{
			$result->path .= '/';

			// auto-double encode, check for '=' sign
			if (strncmp($path, '=', 1) == 0)
			{
				$result->path .= '=' . self::urlencode(substr($path, 1), true);
			}
			else
			{
				$result->path .= self::urlencode($path, true);
			}
		}
		return $result;
	}
	
	/**
	 * Appends a single path parameter to the plug, unencoded.
	 * @note Do not use this method unless you have to(you probably don't).
	 * A real need occurs when initially creating the plug baseuri and an
	 * unencoded "@api" is required.
	 * @see #At() for creating urlencoded paths
	 * 
	 * @return DekiPlug
	 */
	public function AtRaw($path)
	{
		$result = new $this->classname($this, false);

		$result->path .= '/' . $path;

		return $result;	
	}
	
	/**
	 * Add the apikey to the request
	 * 
	 * @return DekiPlug
	 */
	public function WithApiKey()
	{
		return $this->With('apikey', self::getApiKey());
	}

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
			$authToken = null;

			// Deki specific authorization
			// check if there is an authentication token
			$authToken = DekiToken::get();
			if (!is_null($authToken)) 
			{
				// got the token
			}
			else if (isset($headers['X-Authtoken'])) 
			{
				$authToken = $headers['X-Authtoken'];
			}
			
			if (!is_null($authToken)) 
			{
				$authToken = trim($authToken, '"');
				$this->headers['X-Authtoken'] = $authToken;
			} 
			else if (isset($headers['Authorization'])) 
			{
				// Use encoded credentials from the php request header. (e.g. Basic c3lzb3A6c3lzb3A=)
				$this->setHeader('Authorization', $headers['Authorization']);
			}
		}
	}

	// for profiling the api
	protected function preExecuteRequest(&$curl, &$verb, &$content, &$callback)
	{
    	// add a callback to grab additional information from the parse request
    	$callback = array($this, 'postParseRequest');
    	
		$this->requestTimeStart = wfTime();
		$this->requestTimeEnd = null;
		$this->requestVerb = null;
	}

	protected function postExecuteRequest(&$curl, &$verb, &$content, &$response)
	{
		$this->requestTimeEnd = wfTime();
		$this->requestVerb = $verb;
	}
    
	protected function postParseRequest(&$result)
	{
		global $wgPlugProfile;

		$wgPlugProfile[] = array(
			'verb' => $this->requestVerb,
			'url' => $this->getUri(),
			'diff' => ($this->requestTimeEnd - $this->requestTimeStart),
			'stats' => isset($result['headers']['X-Data-Stats']) ?  $result['headers']['X-Data-Stats'] : ''
		);

		return $result;
	}
}
