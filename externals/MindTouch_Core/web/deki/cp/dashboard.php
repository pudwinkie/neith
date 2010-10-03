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

define('DEKI_ADMIN', true);
require_once('index.php');

class Dashboard extends DekiControlPanel
{
	// needed to determine the template folder
	protected $name = 'dashboard';

	public function index()
	{
		$User = DekiUser::getCurrent();

		$upgradetext = '';
		// guerrics: can we move this to the DekiSite object?
		global $wgVersionPingUrl, $IP, $wgProductVersion, $wgIsVM, $wgIsMSI;
		if (!empty($wgVersionPingUrl))
		{
			$Plug = new Plug($wgVersionPingUrl);
			$Plug->timeout = 2; //fail fast
			$Plug = $Plug
				->With('v', $wgProductVersion)
				->With('type', DekiSite::getInstallType())
				->With('license', DekiSite::getStatus())
				->With('commercialtype', DekiSite::getProductType())
				->With('os', PHP_OS);
			$result = $Plug->Get();
			
			if ($result['status'] == 200) {
				$result = unserialize(wfArrayVal($result, 'body'));
				if ($result['sc'] > 0) {
					$upgradetext = $this->View->msgRaw(
						$wgIsVM ? 'Dashboard.upgrade-vm': 'Dashboard.upgrade', 
						$result['v'], 
						'<a href="'.$result['latesturl'].'">'.$result['c'].'</a>'
					);
				}
			}
		}
		
 		$License = DekiLicense::getCurrent();
 		$productType = strtolower(DekiSite::getProductType()); 
 		$this->View->set('product.type', $productType);
		$this->View->set('product.expiration', date('F jS, Y', $License->getExpirationDate()));
		
		$this->View->set('is.commercial', $productType == DekiSite::PRODUCT_COMMERCIAL);
		$this->View->set('product.name', DekiSite::getProductName());
		$this->View->set('product.version', DekiSite::getProductVersion());
		$this->View->set('page.activation', $this->Request->getLocalUrl('product_activation'));
		$this->View->set('page.versions', $this->Request->getLocalUrl('dashboard', 'versions'));
		$this->View->set('upgradetext', $upgradetext);

		$this->View->output();
	}
	
	public function videos()
	{
		global $wgDreamServer;

		$Plug = new DekiPlug($this->Request->getBaseUri());
		$Result = $Plug->At('deki', 'gui', 'videos.php')->With('format', 'php')->With('limit', 10)->Get();

		if (!$Result->isSuccess())
		{
			// couldn't load the videos
			$this->Request->redirect($this->getUrl());
			return;
		}

		$videos = $Result->getAll('body/videos/video');
		$this->View->setRef('videos', $videos);

		$this->View->output();
	}
	
	public function blog() 
	{
		global $wgRssUrl, $wgCacheDirectory;
		
		if (!empty($wgRssUrl)) 
		{
			//cache RSS output
			define('MAGPIE_CACHE_DIR', $wgCacheDirectory);
			require_once('includes/magpie-0.72/rss_fetch.inc');
			$rss = fetch_rss( $wgRssUrl );
			
			$this->View->setRef('posts', $rss->items);
		}
		else
		{
			$this->View->set('posts', array());
		}
		$this->View->output();
	}
	
	/**
	 * Outputs the API's assembly version information
	 */
	public function versions()
	{
		global $wgDreamServer, $wgDreamApi;
		
		$Plug = new DekiPlug($wgDreamServer);
		if (!empty($wgDreamApi))
		{
			$Plug = $Plug->At($wgDreamApi);
		}
		$Result = $Plug->At('host', 'version')->Get();
		
		if ($Result->handleResponse())
		{
			// not going to try and parse this, just pass the result to the view
			$this->View->set('versions', $Result->getAll('body/versions/assembly'));
		}
		
		// set the version name for the subtitle
		$this->View->set('product.name', DekiSite::getProductName());
		$this->View->set('product.version', DekiSite::getProductVersion());
		
		$this->View->output();
	}
}

new Dashboard();
