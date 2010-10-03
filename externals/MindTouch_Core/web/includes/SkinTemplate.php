<?php
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
# 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.
# http://www.gnu.org/copyleft/gpl.html

/**
 * Template-filler skin base class
 * Formerly generic PHPTal (http://phptal.sourceforge.net/) skin
 * Based on Brion's smarty skin
 * Copyright (C) Gabriel Wicke -- http://www.aulinx.de/
 * Copyright (C) MindTouch, Inc.
 *
 * Todo: Needs some serious refactoring into functions that correspond
 * to the computations individual esi snippets need. Most importantly no body
 * parsing for most of those of course.
 *
 * PHPTAL support has been moved to a subclass in SkinPHPTal.php,
 * and is optional. You'll need to install PHPTAL manually to use
 * skins that depend on it.
 *
 * @package MediaWiki
 * @subpackage Skins
 */

/**
 * This is not a valid entry point, perform no further processing unless
 * MEDIAWIKI is defined
 */
if( defined( 'MINDTOUCH_DEKI' ) ) {

require_once 'GlobalFunctions.php';

if (!defined('HPS_SEPARATOR'))
    define( 'HPS_SEPARATOR', '/' );

/**
 * Wrapper object for MediaWiki's localization functions,
 * to be passed to the template engine.
 *
 * @access private
 * @package MediaWiki
 */
class MediaWiki_I18N {
	var $_context = array();

	function set($varName, $value) {
		$this->_context[$varName] = $value;
	}

	function translate($value) {
		$fname = 'SkinTemplate-translate';


		// Hack for i18n:attributes in PHPTAL 1.0.0 dev version as of 2004-10-23
		$value = preg_replace( '/^string:/', '', $value );

		$value = wfMsg( $value );
		// interpolate variables
		while (preg_match('/\$([0-9]*?)/sm', $value, $m)) {
			list($src, $var) = $m;
			wfSuppressWarnings();
			$varValue = $this->_context[$var];
			wfRestoreWarnings();
			$value = str_replace($src, $varValue, $value);
		}

		return $value;
	}
}

/**
 *
 * @package MediaWiki
 */
class SkinTemplate extends Skin {
	/**
	 * @type const - Defines the number of custom HTML areas available
	 */
	const HTML_AREAS = 0;

	/**#@+
	 * @access private
	 */

	/**
	 * Name of our skin, set in initPage()
	 * It probably need to be all lower case.
	 */
	var $skinname;

	/**
	 * Stylesheets set to use
	 * Sub directory in ./skins/ where various stylesheets are located
	 */
	var $stylename;

	/**
	 * For QuickTemplate, the name of the subclass which
	 * will actually fill the template.
	 *
	 * In PHPTal mode, name of PHPTal template to be used.
	 * '.pt' will be automaticly added to it on PHPTAL object creation
	 */
	var $template;

	/**#@-*/

	/**
	 * Setup the base parameters...
	 * Child classes should override this to set the name,
	 * style subdirectory, and template filler callback.
	 *
	 * @param OutputPage $out
	 */
	function initPage( &$out ) {
		parent::initPage( $out );
		$this->skinname  = 'monobook';
		$this->stylename = 'monobook';
		$this->template  = 'QuickTemplate';
	}

	/**
	 * Create the template engine object; we feed it a bunch of data
	 * and eventually it spits out some HTML. Should have interface
	 * roughly equivalent to PHPTAL 0.7.
	 *
	 * @param string $callback (or file)
	 * @param string $repository subdirectory where we keep template files
	 * @param string $cache_dir
	 * @return object
	 * @access private
	 */
	function &setupTemplate( $classname, $repository=false, $cache_dir=false ) {
		$ret_val = new $classname();
		return $ret_val;
	}

	/**
	 * initialize various variables and generate the template
	 *
	 * @param OutputPage $out
	 * @access public
	 */
	function outputPage( &$out ) {
		global $wgTitle, $wgArticle, $wgUser, $wgLang, $wgContLang, $wgOut;
		global $wgScript, $wgStylePath, $wgContLanguageCode;
		global $wgMimeType, $wgOutputEncoding, $wgRequest;
		global $wgLogo, $action, $wgFeedClasses;
		global $wgSitename, $wgScriptPath, $wgLogo, $wgHelpUrl;
		global $wgAnonAccCreate;

		extract( $wgRequest->getValues( 'oldid', 'diff' ) );
		
		$User = DekiUser::getCurrent();
		
		$sk = $wgUser->getSkin();
		
		$this->initPage( $out );
		$tpl =& $this->setupTemplate( $this->template, 'skins' );

		$tpl->setTranslator(new MediaWiki_I18N());
		$this->iseditable = ($wgTitle->getNamespace() != NS_SPECIAL) && !($action == 'edit' || $action == 'submit');
		$this->userpage = $wgContLang->getNsText(NS_USER) . ":" . $wgUser->getName();
		$this->userpageUrlDetails = $this->makeUrlDetails($this->userpage);

		# If user is logged in
		if (!$wgUser->isAnonymous()) {
			$tpl->set('logouturl', $this->makeLogoutUrl());
			$tpl->set('loginurl', '#');
			$tpl->set('registerurl', '');
			$tpl->set('userpageurl', $User->getUrl());
		}
		else {
			$tpl->set('logouturl', '#');
			$tpl->set('loginurl', $this->makeLoginUrl());
			$tpl->set('registerurl', $wgAnonAccCreate ? $this->makeSpecialUrl('Userlogin', 'register=true' ): '');
			$tpl->set('userpageurl', $this->makeSpecialUrl('Userlogin', 'returntomypage=y'));
		}
		
		$tpl->set('title', $wgOut->getPageTitle() );
		$tpl->set('pagetitle', $wgOut->getHTMLTitle() );
		$tpl->set('displaypagetitle', $wgOut->getPageTitle());
		
		$pagetype = array();
		if ($wgTitle->getNamespace() == NS_ADMIN) {
			$pagetype[] = 'page-admin';
		}
		elseif ($wgTitle->getNamespace() == NS_SPECIAL) {
			$pagetype[] = 'page-special';
		}
		elseif ($wgTitle->getNamespace() == NS_USER) {
			$pagetype[] = 'page-user';
		}
		elseif ($wgTitle->getPrefixedText() == wfHomePageInternalTitle()) {
			$pagetype[] = 'page-home';
		}

		if (!$wgUser->isAnonymous()) {
			$pagetype[] = 'user-loggedin';
		}
		if ($wgUser->isAdmin()) {
			$pagetype[] = 'user-admin';
		}
		
		$pagetype[] = 'yui-skin-sam';

		$tpl->set('pagetype', implode(' ', $pagetype));

		$tpl->set('pathcommon', Skin::getCommonPath() );
		$tpl->set('pathtpl', Skin::getTemplatePath()); // skins/$tpl
		$tpl->set('pathskin', Skin::getSkinPath()); // skins/$tpl/$skin
		
		//css
		$tpl->set('resetcss', '<link rel="stylesheet" type="text/css" media="screen" href="'.Skin::getCommonPath().'/reset.css" />');
		$tpl->set('printcss', Skin::getPrintCSS() );
		$tpl->set('screencss', Skin::getScreenCSS() );

		if ($wgRequest->getVal('baseuri')) 
		{
			$wgOut->addHeadHTML('<base target="_top" />');
		}
		$customhead = $wgOut->getHeadHTML();
		$customtail = $wgOut->getTailHTML(); //analytics at the end of the document
		
		$tpl->set('customhead', $customhead);
		$tpl->set('customtail', $customtail);

		//javascripting
		$tpl->set('javascript', Skin::getJavascript() );
		$tpl->set('inlinejavascript', Skin::getEmbeddedJavascript() );

		if ($wgTitle->isEditable()) {
			$tpl->set( 'comments', '<div id="comments">'.$wgOut->getCommentsHTML().'</div>' );
		}

		// MT (steveb): create links to parent pages from title
		$title_hierarchy = $wgTitle->getTitleHierarchy();
		$title_count = count( $title_hierarchy ) - 1;
		$link_title = htmlspecialchars( $title_hierarchy[$title_count]['text'] );
		$tpl->set( 'link_title', $link_title);

		$tpl->setRef( "thispage", $wgTitle->getPrefixedDbKey() );

		if( $wgOut->isSyndicated() ) {
			$feeds = array();
			foreach( $wgFeedClasses as $format => $class ) {
				$feeds[$format] = array(
					'text' => $format,
					'href' => $wgRequest->appendQuery( "feed=$format" ),
					'ttip' => wfMsg('Skin.Common.feed-tooltip-'.$format) // $format=atom
				);
			}
			$tpl->setRef( 'feeds', $feeds );
		} else {
			$tpl->set( 'feeds', false );
		}
		$tpl->setRef( 'mimetype', $wgMimeType );
		$tpl->setRef( 'charset', $wgOutputEncoding );
		global $wgLanguageCode;
		$tpl->set('language', $wgLanguageCode);
		$tpl->set('headlinks', $out->getHeadLinks());
		$tpl->setRef('wgScript', $wgScript);
		$tpl->setRef('skinname', $this->skinname);
		$tpl->setRef('stylename', $this->stylename);
		$tpl->set('loggedin', !$wgUser->isAnonymous());
		$tpl->set('searchaction', $this->escapeSearchLink());
		$tpl->set('search', trim($wgRequest->getVal('search')));
		$tpl->setRef( 'stylepath', $wgStylePath );
		$tpl->set('pagebacklinks', $wgOut->getBacklinksAsList());

		$tpl->setRef('lang', $wgContLanguageCode );
		$tpl->set('langname', $wgContLang->getLanguageName($wgContLanguageCode));
		$tpl->setRef('username', $wgUser->getName());
		$tpl->setRef('userpage', $this->userpage);
		$tpl->set('pageheader', Skin::getPageHeader());
		$tpl->set('pageisrestricted', $wgArticle->isRestricted());
		$tpl->set('pagerevisioncount', $wgArticle->mRevisionCount);
		$tpl->set('hierarchy', $wgArticle->getId() > 0 ? Skin::getHierarchy(): '');
		$tpl->set('hierarchyaslist', $wgArticle->getId() > 0 ? Skin::getHierarchyAsList(): '');
		$tpl->set('pageismoved', $wgOut->getRedirectMessage() != '');
		$tpl->set('pagemovemessage', $wgOut->getRedirectMessage());
		$tpl->set('pagemovelocation', $wgOut->getRedirectLocation());
		$tpl->set('helpurl', $wgHelpUrl);

		$tpl->setRef( 'skin', $this);
		$tpl->set( 'logo', '<a href="'.$this->makeUrl('').'" title="'.htmlspecialchars($wgSitename).'">'
			.'<img src="'.wfGetSiteLogo().'" alt="'.htmlspecialchars($wgSitename).'" title="'.htmlspecialchars($wgSitename).'"/></a>' );

		// define custom HTML areas
		$numRegions = $this->getAreaCount();	
		if ($numRegions > 0)
		{
			$SiteProperties = DekiSiteProperties::getInstance();			
			for ($i = 1; $i <= $numRegions; $i++)
			{
				$tpl->set('customarea'.$i, $SiteProperties->getCustomHtml($i));
			}
		}

		// Set page metrics
		$pageMetrics = $out->getPageMetrics();
		$tpl->set('pageviews', $pageMetrics['views']);
		$tpl->set('pagecharcount', $pageMetrics['charcount']);
		$tpl->set('pagerevisions', $pageMetrics['revisions']);

		# File & image counts
		$tpl->set('filecount', $out->getFileCount());
		$tpl->set('imagecount', $out->getImageCount());
		$tpl->set('tagcount', $out->getTagCount());
		$tpl->set('commentcount', $out->getCommentCount());
		$tpl->set('filedisplaycount', '<span class="unweight" id="fileCount">('.$out->mFileCount.')</span>');
		$tpl->set('imagedisplaycount', '<span class="unweight" id="imageCount">('.$out->mImageCount.')</span>');

		
		//page last modified information
		$lastmod = $this->lastModified('<span class="disabled">'.wfMsg('Skin.Common.page-cant-be-edited').'</span>');
		$tpl->set('lastmod', $lastmod);
		$tpl->set('lastmodby', wfMsg('System.Common.user-nobody'));
		if (0 != $wgArticle->getID() ) {
			$tpl->set('lastmodhuman', $this->lastModifiedHumanReadable());
			if ($wgArticle->getUser()) {
				$tpl->set('lastmodby', $sk->makeLink( $wgContLang->getNsText(NS_USER) . ':' . $wgArticle->getUserText(), $wgArticle->getUserText() ));
			}
		}
		else {
			$tpl->set('viewcount', false);
		}
		$tpl->set('pagemodified', $lastmod);
		$tpl->set('pagemodifiedoffset', $this->lastOffset('<span class="disabled">'.wfMsg('Skin.Common.page-cant-be-edited').'</span>'));
				
		$tpl->set( 'poweredbyico', $this->getPoweredBy() );
		$tpl->set( 'poweredbytext', $this->getPoweredBy() );

		//special case
		$tocData = $out->getTarget('toc');
		$pageToc = '<div class="pageToc"><h5>'.wfMsgForContent('Skin.Common.table-of-contents').'</h5>'.$tocData.'</div>';
		$tpl->set('toc', $pageToc);
		
		if (empty($tocData) || strcmp(strip_tags($tocData), wfMsg('System.API.no-headers')) == 0) {
			$tpl->set('tocexists', false);
			$this->cssclass->pagetoc = 'disabled';
		}
		else {
			$tpl->set('tocexists', true);
			$this->cssclass->pagetoc = '';
		}
		
		if ($wgArticle->userCanAttach()) {
		    $this->onclick->pageattach = 'return doPopupAttach('.$wgTitle->getArticleId().');';
			$this->cssclass->pageattach = '';
		}
		else {
			$this->onclick->pageattach = 'return false';
			$this->cssclass->pageattach = 'disabled';
		}

		if ($wgArticle->userCanEmailPage()) {
			$Title = Title::newFromText(Hooks::SPECIAL_PAGE_EMAIL);
			$this->href->pageemail = $Title->getLocalUrl('pageid='.$wgArticle->getId());
			$this->cssclass->pageemail = '';
		}
		else {
			$this->href->pageemail = '#';
			$this->cssclass->pageemail = 'disabled';
		}

		//not exactly semantically rigorous
		$tpl->set('fileaddlink', Skin::makeNakedLink('#', Skin::iconify('attach').'<span class="text">'. wfMsg('Skin.Common.attach-file-image') .'</span>',
			array('class' => $this->cssclass->pageattach, 'onclick' => $this->onclick->pageattach)));

		if ($wgArticle->userCanEdit())
		{
			$this->onclick->pageedit = 'return Deki.LoadEditor();';
			$this->href->pageedit = $wgTitle->getLocalUrl('action=edit'.($wgRequest->getVal('redirect') == 'no'? '&redirect=no': ''));
			$this->cssclass->pageedit = '';
		}
		else
		{
			$this->cssclass->pageedit = $this->cssclass->pageattach = $this->cssclass->pagetags = 'disabled';
			$this->onclick->pageedit = 'return false';
			$this->href->pageedit = '#';
			$this->cssclass->pageedit = 'disabled';
		}
		
		// view page source
		if ( ($wgArticle->isViewPage() || ($wgArticle->getId() > 0)
			&& $wgArticle->getTitle()->getNamespace() != NS_SPECIAL) )
		{
			$this->onclick->pagesource = '';
			$this->href->pagesource = $wgTitle->getLocalUrl('action=source'.($wgRequest->getVal('redirect') == 'no'? '&redirect=no': ''));
			$this->cssclass->pagesource = '';
		}
		else
		{
			$this->onclick->pagesource = 'return false';
			$this->href->pagesource = '#';
			$this->cssclass->pagesource = 'disabled';			
		}
		
		$this->href->pagetags = '#';
		if ($wgArticle->userCanTag()) {
			$this->onclick->pagetags = 'return doPopupTags(Deki.PageId);';
			$this->cssclass->pagetags = '';
		}
		else {
			$this->onclick->pagetags = 'return false';
			$this->cssclass->pagetags = 'disabled';
		}
		
		//page properties
		if ($wgArticle->userCanSetOptions()) {
			$this->href->pageproperties = $this->makeSpecialUrl('PageProperties', 'id='. $wgArticle->getID());
			$this->cssclass->pageproperties = '';
			$this->onclick->pageproperties = '';
		}
		else {
			$this->href->pageproperties= '#';
			$this->onclick->pageproperties = 'return false';
			$this->cssclass->pageproperties = 'disabled';
		}
		
		if ($wgArticle->userCanRead()) {
			$this->cssclass->pageprint = '';
			$this->onclick->pageprint = 'return window.print();';
			$this->href->pageprint = '#';
		}
		else {
			$this->cssclass->pageprint = 'disabled';
			$this->onclick->pageprint = 'return false';
			$this->href->pageprint = '#';
		}

		if ($wgArticle->userCanRestrict()) 
		{
			$this->cssclass->pagerestrict = '';
			$this->onclick->pagerestrict = '';
			$st = Title::newFromText('PageRestrictions', NS_SPECIAL);
			$this->href->pagerestrict = $st->getLocalUrl('id='.$wgArticle->getId()); 
		}
		else
		{
			$this->href->pagerestrict = '#';
			$this->cssclass->pagerestrict = 'disabled';
			$this->onclick->pagerestrict = 'return false';
		}

		if ($wgArticle->userCanMove()) {
			$this->cssclass->pagemove = '';
			$this->onclick->pagemove = 'return doPopupRename(Deki.PageId, Deki.PageTitle);';
		}
		else {
			$this->cssclass->pagemove = 'disabled';
			$this->onclick->pagemove = 'return false';
		}

		if ($wgArticle->userCanDelete()) {
			$this->cssclass->pagedelete = '';
			$this->onclick->pagedelete = 'return doPopupDelete(Deki.PageId);';
		}
		else {
			$this->cssclass->pagedelete = 'disabled';
			$this->onclick->pagedelete = 'return false';
		}

		if ($wgArticle->userCanCreate() && !$this->isNewPage()) {
			$this->href->pageadd = $wgTitle->getLocalUrl( 'action=addsubpage' );
			$this->cssclass->pageadd = '';
		}
		else {
			$this->href->pageadd = '#';
			$this->cssclass->pageadd = 'disabled';
		}

		//main page link
		$mt = Title::newFromText($wgTitle->getText(), DekiNamespace::getSubject($wgTitle->getNamespace()));
		$this->href->pagemain = $mt->getLocalUrl();
		if ($wgArticle->isViewPage()) {
			$this->cssclass->pagemain = 'active';
		}
		else {
			$this->cssclass->pagemain = 'inactive';
		}
		
		
		$pagewatchtext = '#';
		if ($wgArticle->userCanWatch() && !$wgUser->isAnonymous()) {
			if ($wgTitle->userIsWatching()) {
				$pagewatchtext = wfMsg('Article.Common.action-unwatch');
				$this->href->pagewatch = $wgTitle->getLocalUrl( 'action=unwatch' );
			}
			else {
				$pagewatchtext = wfMsg('Article.Common.action-watch');
				$this->href->pagewatch = $wgTitle->getLocalUrl( 'action=watch' );
			}
			$this->cssclass->pagewatch = '';
		}
		else {
			$this->href->pagewatch = '#';			
			$this->cssclass->pagewatch = 'disabled';
		}

		//talk link
		$ntt = Title::newFromText($wgTitle->getText(), DekiNamespace::getTalk($wgTitle->getNamespace()));
		$this->href->pagetalk = !$wgArticle->userCanTalk() ? '#': $ntt->getLocalUrl();
		if (!$wgArticle->userCanTalk()) 
		{
			$this->cssclass->pagetalk = 'disabled';
		}
		else
		{
			$this->cssclass->pagetalk = DekiNamespace::isTalk($wgTitle->getNamespace()) ? 'active': 'inactive';
		}
		
		//custom buttons to match mediawiki output
		$tpl->set('pagetalklink', $sk->makeLinkObj(
			$ntt,
			wfMsg('Article.Common.talk-link'), 
			'', '', '', 
			$wgArticle->userCanTalk() ?  array('class' => 'selected'): array())
		);
		$nt = Title::newFromText($wgTitle->getText(), NS_MAIN);
		$tpl->set('pagemain', $sk->makeLinkObj(
			$nt, 
			wfMsg('Article.Common.talk-main-link'), 
			'', '', '', 
			$wgArticle->userCanTalk() ?  array('class' => 'selected'): array())
		);
		
		// page alerts: views pages, talk pages, and logged in users with subscribe
		// doesn't make sense for the anon user to subscribe to alerts
		if (($wgArticle->isViewPage() || $wgTitle->isTalkPage()) && !$wgTitle->isTemplateHomepage())
		{
			$enablePageAlerts = !$wgUser->isAnonymous() && $wgUser->canSubscribe();
			$tpl->set('page.alerts', Skin::getPageAlertsButton($wgArticle, $enablePageAlerts));
		}
		
		
		$this->onclick->pagetalk = '';
		$this->onclick->pageemail = '';
		$this->onclick->pageadd = '';
		$this->onclick->pagewatch = '';
		$this->onclick->pagepdf = '';
		$this->onclick->sitetools = 'return DWMenu.Position(\'menuInfo\', this, 0, 5);';
		$this->onclick->pagemore = 'return DWMenu.Position(\'pageMenuContent\', this, 0, -23);';
		$this->onclick->pagetoc = 'return DWMenu.Position(\'menuPageContent\', this, -2, 0)';
		$this->onclick->pagebacklinks = 'return DWMenu.Position(\'menuBacklink\', this, -2, 0);';

		# Define hrefs for common operations
		$this->href->pageattach = '#';
		$this->href->pagemove = '#';
		$this->href->pagedelete = '#';
		$this->href->pagetoc = '#';
		$this->href->pagepdf = $wgArticle->getAlternateContent('application/pdf');
		
		# if prince is not installed, empty hrefs are returned
		if (empty($this->href->pagepdf)) 
		{
			$this->href->pagepdf = '#';
			$this->cssclass->pagepdf = 'disabled';	
		}
		else
		{
			$this->cssclass->pagepdf = '';	
		}

		$tpl->set('pagemain', '<a href="'.$this->href->pagemain.'" class="'.$this->cssclass->pagemain.'" title="'.wfMsg('Skin.Common.view').'">'.wfMsg('Skin.Common.view').'</a>');
		$tpl->set('pagerestrict', '<a href="'.$this->href->pagerestrict.'" title="'.htmlspecialchars(wfMsg('Skin.Common.restrict-access')).'" onclick="'.$this->onclick->pagerestrict.'" class="'.$this->cssclass->pagerestrict.'"><span></span>'.wfMsg('Skin.Common.restrict-access').'</a>');
		$tpl->set('pageattach', '<a href="'.$this->href->pageattach.'" title="'.htmlspecialchars(wfMsg('Skin.Common.attach-file')).'" onclick="'.$this->onclick->pageattach.'" class="'.$this->cssclass->pageattach.'"><span></span>'.wfMsg('Skin.Common.attach-file').'</a>');
		$tpl->set('pagemove', '<a href="'.$this->href->pagemove.'" title="'.htmlspecialchars(wfMsg('Skin.Common.move-page')).'" onclick="'.$this->onclick->pagemove.'" class="'.$this->cssclass->pagemove.'"><span></span>'.wfMsg('Skin.Common.move-page').'</a>');
		$tpl->set('pageedit', '<a href="'.$this->href->pageedit.'" title="'.htmlspecialchars(wfMsg('Skin.Common.edit-page')).'" onclick="'.$this->onclick->pageedit.'" class="'.$this->cssclass->pageedit.'"><span></span>'.wfMsg('Skin.Common.edit-page').'</a>');
		$tpl->set('pagesource', '<a href="'.$this->href->pagesource.'" title="'.htmlspecialchars(wfMsg('Skin.Common.view-page-source')).'" onclick="'.$this->onclick->pagesource.'" class="'.$this->cssclass->pagesource.'"><span></span>'.wfMsg('Skin.Common.view-page-source').'</a>');
		$tpl->set('pageprint', '<a href="'.$this->href->pageprint.'" title="'.htmlspecialchars(wfMsg('Skin.Common.print-page')).'" onclick="'.$this->onclick->pageprint.'" class="'.$this->cssclass->pageprint.'"><span></span>'.wfMsg('Skin.Common.print-page').'</a>');
		$tpl->set('pagepdf', '<a href="'.$this->href->pagepdf.'" title="'.htmlspecialchars(wfMsg('Skin.Common.page-pdf')).'" class="'.$this->cssclass->pagepdf.'"><span></span>'.wfMsg('Skin.Common.page-pdf').'</a>');
		$tpl->set('pagedelete', '<a href="'.$this->href->pagedelete.'" title="'.htmlspecialchars(wfMsg('Skin.Common.delete-page')).'" onclick="'.$this->onclick->pagedelete.'" class="'.$this->cssclass->pagedelete.'"><span></span>'.wfMsg('Skin.Common.delete-page').'</a>');
		$tpl->set('pageadd', '<a href="'.$this->href->pageadd.'" title="'.htmlspecialchars(wfMsg('Skin.Common.new-page')).'" class="'.$this->cssclass->pageadd.'"><span></span>'.wfMsg('Skin.Common.new-page').'</a>');
		$tpl->set('pagetoc', '<a href="'.$this->href->pagetoc.'" title="'.htmlspecialchars(wfMsg('Skin.Common.table-of-contents')).'" onclick="'.$this->onclick->pagetoc.'" class="'.$this->cssclass->pagetoc.'"><span></span>'.wfMsg('Skin.Common.table-of-contents').'</a>');
		$tpl->set('pagetalk', '<a href="'.$this->href->pagetalk.'" title="'.htmlspecialchars(wfMsg('Skin.Common.page-talk')).'" onclick="'.$this->onclick->pagetalk.'" class="'.$this->cssclass->pagetalk.'"><span></span>'.wfMsg('Skin.Common.page-talk').'</a>');
		$tpl->set('pageemail', '<a href="'.$this->href->pageemail.'" title="'.htmlspecialchars(wfMsg('Skin.Common.email-page')).'" onclick="'.$this->onclick->pageemail.'"><span></span>'.wfMsg('Skin.Common.email-page').'</a>');
		$tpl->set('pagewatch', '<a href="'.$this->href->pagewatch.'" title="'.htmlspecialchars($pagewatchtext).'" onclick="'.$this->onclick->pagewatch.'"><span></span>'.$pagewatchtext.'</a>');
 		$tpl->set('pagename', $wgTitle->getClassNameText());
 		$tpl->set('pageproperties', '<a href="'.$this->href->pageproperties.'" title="'.htmlspecialchars(wfMsg('Skin.Common.page-properties')).'" onclick="'.$this->onclick->pageproperties.'"><span></span>'.wfMsg('Skin.Common.page-properties').'</a>');
 		
		$pageFooter = $this->afterContent();

		// pages sometimes requires subnavigation elements
		$subnav = $out->getSubNavigation();
		$tpl->set('pagesubnav', !empty($subnav) ? '<div class="deki-page-subnav">'.$subnav.'</div>': '');
		
		//append some extra divs for identifying special/admin pages
		$bodyText = $out->mBodytext;
		if ($wgTitle->getNamespace() == NS_ADMIN) {
			$bodyText = '<div id="pageTypeAdmin">'.$bodyText.'</div>';
		}
		elseif ($wgTitle->getNamespace() == NS_SPECIAL) {
			$bodyText = '<div id="pageTypeSpecial">'.$bodyText.'</div>';
		}
		$tpl->set('bodytext', '<div id="page-top"><div id="pageToc">'.$pageToc.'</div><div class="pageText" id="pageText">'.$bodyText.'</div></div>'.$pageFooter );

		// load the navigation pane from a plugin
		$navText = '';
		DekiPlugin::executeHook(Hooks::SKIN_NAVIGATION_PANE, array($wgTitle, &$navText));
		$tpl->set('sitenavtext', $navText);

		if ($wgTitle->isEditable() && (Skin::isViewPage() || Skin::isEditPage()) && 0 != $wgArticle->getID()) {
			$tpl->set('filestext', strlen($out->mFilestext) > 0? $out->mFilestext : '<div class="nofiles">&nbsp;</div>');
			$tpl->set('gallerytext', $out->mGallerytext );

			//set tagging templating variable
			$tpl->set('tagstext', '<div id="deki-page-tags">'.$out->mTagstext.'</div>' );
			
			// set related pages
			$tpl->set('related', Skin::getRelatedPages());
		}
		else {
			$tpl->set('filestext', '<div class="nofiles">&nbsp;</div>');
			$tpl->set('gallerytext', '');
			$tpl->set('tagstext', '');
			$tpl->set('related', '');
		}

		# Language links
		$language_urls = array();
		foreach( $wgOut->getLanguageLinks() as $l ) {
			$nt = Title::newFromText( $l );
			$language_urls[] = array('href' => $nt->getFullURL(),
			'text' => ($wgContLang->getLanguageName( $nt->getInterwiki()) != ''?$wgContLang->getLanguageName( $nt->getInterwiki()) : $l),
			'class' => $wgContLang->isRTL() ? 'rtl' : 'ltr');
		}
		if(count($language_urls)) {
			$tpl->setRef( 'language_urls', $language_urls);
		} else {
			$tpl->set('language_urls', false);
		}
		$tpl->set('pagefooter', Skin::getPageFooter());

		//overrides from content
		$targets = $wgOut->getTargets();
		foreach ($targets as $key => $val) 
		{
			//special case; we've already done some magical formatting around toc
			if ($key == 'toc') 
			{
				continue;
			}
			$tpl->set($key, $val);
		}
		
		//overrides from LocalSettings.php
		$this->setTemplateOverrides($tpl);
		
		$res = $tpl->execute();
		
		// result may be an error
		$this->printOrError( $res );

	}

	/***
	 * Values in LocalSettings.php can override your template variables
	 */
	function setTemplateOverrides(&$tpl) {
		global $wgTemplateOverrides;
		foreach ($wgTemplateOverrides as $key => $val) {
			$tpl->set($key, $val);
		}
	}

	/**
	 * Output the string, or print error message if it's
	 * an error object of the appropriate type.
	 * For the base class, assume strings all around.
	 *
	 * @param mixed $str
	 * @access private
	 */
	function printOrError( &$str ) {
		echo $str;
	}

	/**
	 * @return int - the number of HTML_AREAS defined for this template
	 */
	private function getAreaCount()
	{
		$class = get_class($this);
		$constant = $class.'::HTML_AREAS';
		return defined($constant) ? constant($constant) : 0;
	}
}

/**
 * Generic wrapper for template functions, with interface
 * compatible with what we use of PHPTAL 0.7.
 */
class QuickTemplate {
	/**
	 * @access public
	 */
	function QuickTemplate() {
		$this->data = array();
		$this->translator = new MediaWiki_I18N();
	}

	/**
	 * @access public
	 */
	function set( $name, $value ) {
		$this->data[$name] = $value;
	}

	/**
	 * @access public
	 */
	function setRef($name, &$value) {
		$this->data[$name] =& $value;
	}

	/**
	 * @access public
	 */
	function setTranslator( &$t ) {
		$this->translator = &$t;
	}

	/**
	 * @access public
	 */
	function execute() {
		echo "Override this function.";
	}
	
	/**
	 * Perform any needed dashboard customizations, disabling of template variables, etc.
	 * @access public
	 */
	function setupDashboard()
	{
		return;
	}

	/**
	 * @access private
	 */
	function text( $str ) {
		if (isset($this->data[$str])) {
			echo htmlspecialchars( $this->data[$str] );
		}
	}


	/**
	 * @access private
	 */
	function textString( $str ) {
		if (isset($this->data[$str])) {
			return htmlspecialchars( $this->data[$str] );
		}
	}

	/**
	 * @access private
	 */
	function html( $str ) {
		if (isset($this->data[$str])) {
			echo $this->data[$str];
		}
	}

	/**
	 * @access private
	 */
	function msg( $str ) {
		echo htmlspecialchars( $this->translator->translate( $str ) );
	}

	function msgHtml( $str ) {
		echo $this->translator->translate( $str );
	}

	function haveData( $str ) {
		return $this->data[$str];
	}
	
	function hasData($str) {
		return !empty($this->data[$str]);
	}	

	function haveOnClick($str) {
		$sk = $this->data['skin'];
		return $sk->onclick->$str;
	}

	function haveCssClass($str) {
		$sk = $this->data['skin'];
		return $sk->cssclass->$str;
	}

	function haveHref($str) {
		$sk = $this->data['skin'];
		return $sk->href->$str;
	}

	function haveMsg( $str ) {
		$msg = $this->translator->translate( $str );
		return ($msg != '-') && ($msg != ''); # ????
	}
}

} // end of if( defined( 'MINDTOUCH_DEKI' ) )
?>
