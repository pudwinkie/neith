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
 
class DekiSite 
{
	const PRODUCT_CORE = 'community';
	const PRODUCT_PLATFORM = 'platform';
	const PRODUCT_COMMERCIAL = 'commercial';
	
	const STATUS_COMMUNITY = 'COMMUNITY';
	const STATUS_TRIAL = 'TRIAL';
	const STATUS_COMMERCIAL = 'COMMERCIAL';
	const STATUS_INVALID = 'INVALID';
	const STATUS_INACTIVE = 'INACTIVE';
	const STATUS_EXPIRED = 'EXPIRED';
	
	private static $PRODUCTS = array(
		self::PRODUCT_CORE => 'MindTouch Core v10', 
		self::PRODUCT_PLATFORM => 'MindTouch Platform v10', 
		self::PRODUCT_COMMERCIAL => 'MindTouch 2010'
	);
	
	// @var string - stores the computed site status
	private static $siteStatus = null;
	
	/**
	 * Get the set sitename
	 * @return string
	 */
	public static function getName() 
	{
		global $wgSitename;
		return $wgSitename;	
	}
		
	/**
	 * Returns the technical product version
	 * @return string
	 */
	public static function getProductVersion()
	{
		global $wgProductVersion;
		return $wgProductVersion;
	}
	 
	/**
	 * Returns the user-friendly product name (no link)
	 * @return string
	 */
	public static function getProductName()
	{	
		return self::$PRODUCTS[self::getProductType()]; 
	}
	
	/**
	 * Returns the user-displayed product version, with a link to MindTouch.com
	 * @return string
	 */
	public static function getProductLink()
	{
		global $wgProductVersion;
		
		$name = self::getProductName();
		
		// title attribute
		$title = $name; 
		if (self::isCore())
		{
			$title .= wfMsg('Product.version', $wgProductVersion); 
		}
		else
		{
			$title .= wfMsg('Product.version.commercial', $wgProductVersion);
		}
		
		$suffix = ''; 
		if (self::isTrial())
		{
			$suffix = wfMsg('Product.type.trial'); 	
		}
		else if (self::isExpired())
		{
			$suffix = wfMsg('Product.type.expired');	
		}
		else if (self::isInactive())
		{
			$suffix = wfMsg('Product.type.inactive');	
		}
		if (!empty($suffix))
		{
			$suffix = ' <span class="product-suffix">('.$suffix.')</span>'; 	
		}
		
		return '<a href="'.ProductURL::HOMEPAGE.'" class="product" title="'.htmlspecialchars($title).'">'.$name.$suffix.'</a>';
	}
	
	/**
	 * Returns the type of install you're using
	 * @return string
	 */
	public static function getInstallType() 
	{
		global $IP, $wgHostedVersion; 
		
		$wgIsVM = $wgIsMSI = $wgIsAMI = $wgIsLinuxPkg = $wgIsEsxVM = false;
		$checkfiles = array('vm.php', 'msi.php', 'installtype.ami.php', 'installtype.package.php', 'installtype.vmesx.php'); 
		foreach ($checkfiles as $file) 
		{
			if (is_file($IP.'/config/'.$file)) 
			{
				require_once($IP.'/config/'.$file); 
			}	
		}

		$type = 'source';
		if (isset($wgIsVM) && $wgIsVM) 
		{
			$type = 'vm';
		}
		if (isset($wgIsEsxVM) && $wgIsEsxVM) 
		{
			$type = 'vmesx';
		}
		if (isset($wgHostedVersion) && $wgHostedVersion) 
		{
			$type = 'hosted';
		}
		if (isset($wgIsMSI) && $wgIsMSI) 
		{
			$type = 'msi';	
		}
		if (isset($wgIsPackage) && $wgIsPackage) 
		{
			$type = 'package';
		}
		if (isset($wgIsAMI) && $wgIsAMI) 
		{
			$type = 'ami';
		}
		return $type;
	}
	
	/**
	 * Retrieves the current site timezone offset
	 * @return string
	 */
	public static function getTimezoneOffset()
	{
		global $wgDefaultTimezone;
		$timezone = wfGetConfig('ui/timezone', $wgDefaultTimezone);
		
		// backwards compat
		if ($timezone == '00:00')
		{
			$timezone = '+00:00';
		}

		return $timezone;
	}

	/**
	 * Fetch an array of timezone options for use with select inputs
	 * @return array
	 */
	public static function getTimeZoneOptions()
	{
		global $wgCustomTimezones;
		$ntimezones = array();
		$ptimezones = array();
		foreach ($wgCustomTimezones as $tz) 
		{
			$tz = validateTimeZone($tz);
			if (strncmp($tz, '-', 1) == 0) 
			{
				$ntimezones[] = $tz;
			}
			else 
			{
				$ptimezones[] = $tz;
			}
		}
		for ($i = -12; $i < 14; $i++) 
		{
			$val = validateTimeZone($i.':00');
			if (strncmp($val, '-', 1) == 0) 
			{
				$ntimezones[] = $val;
			}
			else 
			{
				$ptimezones[] = $val;
			}
		}
		rsort($ntimezones);
		sort($ptimezones);
		$timezones = array_merge($ntimezones, $ptimezones);
		
		$time = gmmktime();
		
		$match = null;
		$options = array();

		foreach ($timezones as $timezone)
		{
			//parse out to do time transition
			preg_match("/([-+])([0-9]+):([0-9]+)/", $timezone, $match);
			$offset = ($match[2] * 3600 + $match[3] * 60) * (strcmp($match[1], '-') == 0 ? -1 : 1);
			
			$displayTimezone = ($timezone == '+00:00') ? '00:00' : $timezone;
			
			$tztime = gmdate('h:i A', $time + $offset);

			$options[$timezone] = wfMsg('System.Common.timezone-display', $tztime, $displayTimezone);
		}
		
		return $options;
	}
	
	/**
	 * Get the product type (as defined by constants above)
	 * @return string
	 */
	public static function getProductType()
	{
		$License = DekiLicense::getCurrent();
		
		// is it a core? easy-peasy
		if ($License->getLicenseType() == self::PRODUCT_CORE)
		{
			return self::PRODUCT_CORE;
		}
		else
		{
			// if we're returning a commercial license, check the license first then fall back to platform
			return $License->getCommercialNameKey(self::PRODUCT_PLATFORM);
		}
	}
	
	/***
	 * Tickle the package import service
	 * @return DekiResult
	 */
	 public static function refreshPackages()
	 {
		 // tickle package importer service
		global $wgApi, $wgDekiApi, $wgDekiSiteId, $wgRequest;
		
		$DekiPlug = DekiPlug::getInstance();
		$Request = DekiRequest::getInstance();
		$SiteProperties = DekiSiteProperties::getInstance(); 
		
		// clear out all the old package properties to trigger the reload
		$SiteProperties->clearAllPackages(); 
		
		$dekiPath = $wgApi . ($wgDekiApi{0} != '/' ? '/': '') . $wgDekiApi; 
		
		// tickle the template updater service
		$data = array(
			'update' => array(
				'@wikiid' => $wgDekiSiteId, 
				'uri' => $Request->getScheme() . '://' . $Request->getHost() . '/' . $dekiPath
			)
		);
		$Result = $DekiPlug->At('packageupdater', 'update')->WithApiKey()->Post($data); 
		
		return $Result;
	 }
	
	/**
	 * Get the status of the site as reported by site/settings
	 * @return string
	 */
	public static function getStatus()
	{
		if (is_null(self::$siteStatus))
		{
			self::$siteStatus = wfGetConfig('license/state/#text', self::STATUS_COMMUNITY);
			
			// override the returned status if the site is in expiration
			if (self::willExpire() < 0)
			{
				self::$siteStatus = self::STATUS_EXPIRED;
			}
		}
		
		return self::$siteStatus;
	}
	/**
	 * Is this a core install?
	 * @return bool
	 */
	public static function isCore() { return self::getStatus() == self::STATUS_COMMUNITY; }
	/**
	 * Is this a trial instance?
	 * @return bool
	 */
	public static function isTrial() { return self::getStatus() == self::STATUS_TRIAL; }
	/**
	 * Is this a commercial instance?
	 * @return bool
	 */
	public static function isCommercial()  { return in_array(self::getStatus(), array(self::STATUS_COMMERCIAL, self::STATUS_TRIAL)); }
	/**
	 * Is this an invalid instance?
	 * @return bool
	 */
	public static function isInvalid() { return self::getStatus() == self::STATUS_INVALID; }
	/**
	 * Is this an inactive instance?
	 * @return bool
	 */
	public static function isInactive() { return self::getStatus() == self::STATUS_INACTIVE; }
	/**
	 * Is this an expired instance?
	 * @return bool
	 */
	public static function isExpired() { return self::getStatus() == self::STATUS_EXPIRED; }
	/**
	 * Is the site in any type of non-functioning state due to licensing restrictions?
	 * @return bool
	 */
	public static function isDeactivated() { return self::isInvalid() || self::isExpired() || self::isInactive(); }

	/**
	 * Will this site expire soon? (And if so, in how many days?)
	 * @note days can be negative if the site is already expired!
	 * 
	 * @return mixed - false or int (days to expire)
	 */
	public static function willExpire() 
	{
		global $wgUser;
		
		$expiry = wfGetConfig('license/expiration/#text', null);
		if (is_null($expiry)) 
		{
			return false;
		}
		
		if ($wgUser->isAdmin())
		{
			global $wgShowBannerToAdmins; 
			$days = $wgShowBannerToAdmins; 
		}
		else if (!$wgUser->isAnonymous())
		{
			global $wgShowBannerToUsers; 
			$days = $wgShowBannerToUsers; 
		}
		else
		{
			global $wgShowBannerToAnon; 
			$days = $wgShowBannerToAnon; 
		}
		
		$timestamp = wfTimestamp(TS_UNIX, $expiry);
		$diff = $timestamp - mktime();
		
		if ($diff > ($days * 86400)) 
		{
			return false;
		}
		
		return ceil($diff / 86400);
	}
}
