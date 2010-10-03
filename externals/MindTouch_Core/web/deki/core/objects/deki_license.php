<?php
/*
 * MindTouch Deki - enterprise collaboration and integration platform
 * Copyright (C) 2006-2009 MindTouch, Inc.
 * www.mindtouch.com  oss@mindtouch.com
 *
 * For community documentation and downloads visit www.opengarden.org;
 * please review the licensing section.
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

 
class DekiLicense 
{
	protected static $instance = null;
	
	const CAPABILITY_SEARCH = 'search-engine';
	const SEARCH_ADAPTIVE = 'adaptive';
	
	const CAPABILITY_MESSAGING = 'commercial-messaging'; 
	const MESSAGING_SUPPRESSED = 'off';
	
	const CAPABILITY_ANON = 'anonymous-permissions';
	const ANON_PERMISSIONS = 'ALL';
	
	const CAPABILITY_CACHE = 'cache-provider';
	const CACHE_MEMCACHE = 'memcache';
	const CACHE_MEMORY = 'inprocess';
	
	const CAPABILITY_RATING = 'content-rating';
	const RATING = 'enabled';
	
	const PRODUCT_KEY = 'commercial-type';
	const PRODUCT_NAME = 'commercial-name-override';
	
	/** 
	 * @var DekiResult
	 */
	protected $License;
	protected $capabilities;
	
	/**
	 * Return the current instance of DekiLicense
	 * @return DekiLicense
	 */
	static function &getCurrent()
	{
		if (is_null(self::$instance))
		{
			self::$instance = new self();
		}

		return self::$instance;
	}
	
	/**
	 * Returns the embedded MSA in the license
	 * This is a static method because it requires a separate plug call so we can retrieve in a string (not php) format
	 * @return string
	 */
	public static function getEmbeddedMSA()
	{
		global $wgDekiApi, $wgDreamServer;
		
		// generate a new plug cause we can't use php output format
		$Plug = new DekiPlug($wgDreamServer, null);
		$Result = $Plug->AtRaw($wgDekiApi)->At('license')->WithApiKey()->Get();
		$body = $Result->getVal('body');
		
		$license = '';
		if (preg_match("/<support-agreement type=\"xhtml\">(.*)<\/support-agreement>/is", $body, $matches))
		{
			$license = $matches[1];
		}
		if (preg_match("/<source-license type=\"xhtml\">(.*)<\/source-license>/is", $body, $matches))
		{
			$license .= $matches[1];
		}
		return $license;
	}
	
	/**
	 * Returns the license type
	 * @return string
	 */
	public function getLicenseType()
	{
		return strtolower($this->License->getVal('body/license.private/@type', DekiSite::PRODUCT_CORE)); 	
	}
	
	/**
	 * Get information about the licensee
	 * @return array
	 */
	public function getLicensee()
	{
		return $this->License->getVal('body/license.private/licensee', array());
	}
	
	/**
	 * Get information about the primary contact for this license
	 * @return array
	 */
	public function getPrimaryContact()
	{
		return $this->License->getVal('body/license.private/contact.primary', array());
	}
	
	/**
	 * Get information about the secondary contact for this license
	 * @return array
	 */
	public function getSecondaryContact()
	{
		return $this->License->getVal('body/license.private/contact.secondary', array());
	}
	
	/**
	 * Get the site count (total multi-tenants)
	 * @return array
	 */
	public function getSiteCount()
	{
		return $this->License->getVal('body/license.private/grants/active-sites');
	}
	
	/**
	 * Get the total user count allowed
	 * @return array
	 */
	public function getUserCount()
	{
		return $this->License->getVal('body/license.private/grants/active-users');
	}
	
	/**
	 * Get all valid hosts
	 * @return array
	 */
	public function getHosts()
	{
		return $this->License->getAll('body/license.private/license.public/host', array());
	}
	
	/**
	 * Get expiration date of license
	 * @param string $format - format the timestamp to a different format than the one provided by the license
	 * @return string
	 */
	public function getExpirationDate($format = TS_UNIX)
	{
		$expiration = $this->License->getVal('body/license.private/date.expiration');
		if (!is_null($format) && !is_null($expiration))
		{
			$expiration = wfTimestamp($format, $expiration);
		}
		return $expiration;
	}
	
	/**
	 * Get issue date of license
	 * @param string $format - format the timestamp to a different format than the one provided by the license
	 * @return string
	 */
	public function getIssuedDate($format = TS_UNIX)
	{
		$issued = $this->License->getVal('body/license.private/date.issued');
		if (!is_null($format))
		{
			$issued = wfTimestamp($format, $issued);
		}
		return $issued;
	}
	
	/**
	 * Get a list of active SIDs as allowed by the license
	 * @return array - sids as key, expiration as val
	 */
	public function getSids()
	{
		$sids = array();
		$services = $this->License->getAll('body/license.private/grants/service-license', array());
		$licenseExpiration = $this->getExpirationDate(null);
		foreach ($services as $service)
		{
			// grab the license service expiration if one isn't set specifically for this one
			$expiration = !isset($service['@date.expire']) ? $licenseExpiration : $service['@date.expire'];
			$sids[$service['@sid']] = !is_null($expiration) ? wfTimestamp(TS_UNIX, $expiration): $expiration;
		}
		return $sids;
	}
	
	/**
	 * Returns the search capability as enabled by the license
	 * @return bool
	 */
	public function hasCapabilitySearch()
	{
		return $this->getCapability(self::CAPABILITY_SEARCH) == self::SEARCH_ADAPTIVE;	
	}
	
	/**
	 * Returns the anon user's permissions as enabled by the license
	 * @return bool
	 */
	public function hasCapabilityAnon()
	{
		return $this->getCapability(self::CAPABILITY_ANON) == self::ANON_PERMISSIONS;	
	}
	
	/** 
	 * Read the license and see if we should be displaying commercial messaging to this install
	 *
	 * @TODO royk this should be expanded in the future to be smarter - for now, we can make some 
	 * assumptions about platform + 2010 to simplify this.
	 * 
	 */
	public function displayCommercialMessaging()
	{
		
		return $this->getCapability(self::CAPABILITY_MESSAGING) != self::MESSAGING_SUPPRESSED; 
	}
	
	/**
	 * Returns the cache mechanism as enabled by the license
	 * @return bool
	 */
	public function hasCapabilityMemCache()
	{
		return $this->getCapability(self::CAPABILITY_CACHE) == self::CACHE_MEMCACHE;	
	}
	
	/**
	 * Returns the cache mechanism as enabled by the license
	 * @return bool
	 */
	public function hasCapabilityCaching()
	{
		return $this->getCapability(self::CAPABILITY_CACHE) == self::CACHE_MEMORY
		 || $this->hasCapabilityMemCache();	
	}
	
	/**
	 * Returns the capabilities as enabled by the API
	 * @return bool
	 */
	public function hasCapabilityRating()
	{
		return $this->getCapability(self::CAPABILITY_RATING) == self::RATING;	
	}
	
	/**
	 * Returns a key (see constants on DekiSite) of the product type
	 * @param string $default default state of the commercial key
	 * @return mixed string or null
	 */
	public function getCommercialNameKey($default)
	{
		return $this->getCapability(self::PRODUCT_KEY, $default);	
	}
	
	/**
	 * Get the value for a specific capability
	 * @param string $key capability key
	 * @param string $return the return value, if the key's value isn't set 
	 * @return string
	 */
	protected function getCapability($key, $return = null)
	{
		return isset($this->capabilities[$key]) ? $this->capabilities[$key] : $return;
	}
			
	/**
	 * Load the license from the API with the API key
	 */
	protected function load()
	{
		$Plug = DekiPlug::getInstance(); 
		$License = $Plug->At('license')->WithApiKey()->Get();
		
		// set capabilities
		$grants = $License->getVal('body/license.private/grants', array());
		if (is_array($grants))
		{
			unset($grants['service-license']); //ignore sids
			unset($grants['active-sites']); //ignore # of sites
			unset($grants['active-users']); //ignore # of users
		}
		
		// set allowed capabilities
		$this->capabilities = $grants;
		
		// set the license result object
		$this->License = $License;		
	}
	
	protected function __construct()
	{
		$this->load();
	}
}
