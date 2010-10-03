
-- Default configuration values --
insert into `config` (`config_key`, `config_value`) values('storage/s3/prefix', "");
insert into `config` (`config_key`, `config_value`) values('storage/s3/bucket', "");
insert into `config` (`config_key`, `config_value`) values('storage/s3/privatekey', "");
insert into `config` (`config_key`, `config_value`) values('storage/s3/publickey', "");
insert into `config` (`config_key`, `config_value`) values('storage/type', 'fs');
insert into `config` (`config_key`, `config_value`) values('storage/s3/timeout', '');
insert into `config` (`config_key`, `config_value`) values('files/max-file-size','268435456');
insert into `config` (`config_key`, `config_value`) values('files/blocked-extensions','html, htm, exe, vbs, scr, reg, bat, com, htm, html, xhtml');
insert into `config` (`config_key`, `config_value`) values('files/force-text-extensions','htm, html, xhtml, bat, reg, sh');
insert into `config` (`config_key`, `config_value`) values('files/blacklisted-disposition-mimetypes','');
insert into `config` (`config_key`, `config_value`) values('files/whitelisted-disposition-mimetypes','text/plain, text/xml, application/xml, application/pdf, application/msword, application/vnd.ms-excel, application/vnd.ms-powerpoint, application/vnd.openxmlformats-officedocument.wordprocessingml.document, application/vnd.openxmlformats-officedocument.spreadsheetml.sheet, application/vnd.openxmlformats-officedocument.presentationml.presentation, application/vnd.oasis.opendocument.presentation, application/vnd.oasis.opendocument.spreadsheet, application/vnd.oasis.opendocument.text, application/x-shockwave-flash');
insert into `config` (`config_key`, `config_value`) values('files/imagemagick-extensions','bmp, jpg, jpeg, png, gif');
insert into `config` (`config_key`, `config_value`) values('files/imagemagick-max-size','2000000');
insert into `config` (`config_key`, `config_value`) values('ui/banned-words',"");
insert into `config` (`config_key`, `config_value`) values('ui/sitename','MindTouch');
insert into `config` (`config_key`, `config_value`) values('ui/language','en-us');
insert into `config` (`config_key`, `config_value`) values('security/new-account-role','Contributor');
insert into `config` (`config_key`, `config_value`) values('cache/master','request');
insert into `config` (`config_key`, `config_value`) values('security/cookie-expire-secs','604800');
insert into `config` (`config_key`, `config_value`) values('security/allow-anon-account-creation','true');
insert into `config` (`config_key`, `config_value`) values('editor/safe-html','true');
insert into `config` (`config_key`, `config_value`) values('languages',"");
insert into `config` (`config_key`, `config_value`) values('ui/user-dashboards', 'Template:MindTouch/Views/ActivityDashboard, user_page');

-- Default pages --
INSERT INTO `pages` (`page_namespace`, `page_title`, `page_text`, `page_comment`, `page_timestamp`, `page_touched`, `page_toc`, `page_tip`, `page_parent`, `page_restriction_id`, `page_content_type`, `page_display_name`) VALUES 
	(101, '', '{{ wiki.tree("Special:") }}', '', DATE_FORMAT(NOW(), "%Y%m%d%H%i%s"), '', '', 'Special page', 0, 2, 'application/x.deki-text', ''),
	(101, 'Userlogin', '', '', DATE_FORMAT(NOW(), "%Y%m%d%H%i%s"), '', '', 'Special page', 0, 0, 'text/plain', ''),
	(101, 'Userlogout', '', '', DATE_FORMAT(NOW(), "%Y%m%d%H%i%s"), '', '', 'Special page', 0, 0, 'text/plain', ''),
	(101, 'Preferences', '', '', DATE_FORMAT(NOW(), "%Y%m%d%H%i%s"), '', '', 'Special page', 0, 0, 'text/plain', ''),
	(101, 'Watchedpages', '', '', DATE_FORMAT(NOW(), "%Y%m%d%H%i%s"), '', '', 'Special page', 0, 0, 'text/plain', ''),
	(101, 'Recentchanges', '', '', DATE_FORMAT(NOW(), "%Y%m%d%H%i%s"), '', '', 'Special page', 0, 0, 'text/plain', ''),
	(101, 'Listusers', '', '', DATE_FORMAT(NOW(), "%Y%m%d%H%i%s"), '', '', 'Special page', 0, 0, 'text/plain', ''),
	(101, 'ListTemplates', '', '', DATE_FORMAT(NOW(), "%Y%m%d%H%i%s"), '', '', 'Special page', 0, 0, 'text/plain', ''),
	(101, 'ListRss', '', '', DATE_FORMAT(NOW(), "%Y%m%d%H%i%s"), '', '', 'Special page', 0, 0, 'text/plain', ''),
	(101, 'Search', '', '', DATE_FORMAT(NOW(), "%Y%m%d%H%i%s"), '', '', 'Special page', 0, 0, 'text/plain', ''),
	(101, 'Sitemap', '', '', DATE_FORMAT(NOW(), "%Y%m%d%H%i%s"), '', '', 'Special page', 0, 0, 'text/plain', ''),
	(101, 'Contributions', '', '', DATE_FORMAT(NOW(), "%Y%m%d%H%i%s"), '', '', 'Special page', 0, 0, 'text/plain', ''),
	(101, 'Undelete', '', '', DATE_FORMAT(NOW(), "%Y%m%d%H%i%s"), '', '', 'Special page', 0, 0, 'text/plain', ''),
	(101, 'Popularpages', '', '', DATE_FORMAT(NOW(), "%Y%m%d%H%i%s"), '', '', 'Special page', 0, 0, 'text/plain', ''),
	(101, 'Watchlist', '', '', DATE_FORMAT(NOW(), "%Y%m%d%H%i%s"), '', '', 'Special page', 0, 0, 'text/plain', ''),
	(101, 'About', '', '', DATE_FORMAT(NOW(), "%Y%m%d%H%i%s"), '', '', 'Special page', 0, 0, 'text/plain', ''),
	(101, 'Statistics', '', '', DATE_FORMAT(NOW(), "%Y%m%d%H%i%s"), '', '', 'Special page', 0, 0, 'text/plain', ''),
	(101, 'Tags', '', '', DATE_FORMAT(NOW(), "%Y%m%d%H%i%s"), '', '', 'Special page', 0, 0, 'text/plain', ''),
	(101, 'Events', '', '', DATE_FORMAT(NOW(), "%Y%m%d%H%i%s"), '', '', 'Special page', 0, 0, 'text/plain', ''),
	(2, '', 'User page', '', DATE_FORMAT(NOW(), "%Y%m%d%H%i%s"), '', '', 'Admin page', 0, 0, 'text/plain', ''),
	(10, '', '', '', DATE_FORMAT(NOW(), "%Y%m%d%H%i%s"), '', '', 'Template page', 0, 0, 'text/plain', '');

-- OOBE --
INSERT into `pages` (`page_namespace`, `page_title`, `page_text`, `page_comment`, `page_touched`, `page_user_id`, `page_timestamp`, `page_toc`, `page_tip`, `page_restriction_id`, `page_display_name`)
    VALUES (0, '', '<pre class="script">if (!wiki.pageexists(&quot;Template:MindTouch/Views/Admin_Tabs&quot;) || !wiki.pageexists(&quot;Template:MindTouch/Views/User_Tabs&quot;)) { &lt;div class=&quot;mt-populating&quot;&gt;&lt;h2&gt;&quot;Wait a moment while we complete your installation...&quot;;&lt;/h2&gt;&lt;p&gt;&quot;MindTouch is populating all the necessary templates and pages to make your site fully functional.&quot;;&lt;/p&gt;&lt;p&gt;&quot;While you wait, you can still use the site normally - feel free to navigate or create new pages.&quot;;&lt;/p&gt;&lt;p&gt;&quot;Once we finish populating your site with content, this message will disappear (be sure to refresh this page!)&quot;;&lt;/p&gt;&lt;/div&gt;} else {&lt;h2&gt;&quot;Welcome to MindTouch&quot;;&lt;/h2&gt;if (user.admin) {wiki.template(&quot;MindTouch/Views/Admin_Tabs&quot;);} else {wiki.template(&quot;MindTouch/Views/User_Tabs&quot;);}}</pre>', '', 0, 1, DATE_FORMAT(NOW(), "%Y%m%d%H%i%s"), '', '', 0, '');

-- Default restrictions --
INSERT INTO `restrictions` (`restriction_name`, `restriction_perm_flags`, `restriction_creator_user_id`) VALUES
	('Public', '18446744073709551615', '1'),
	('Semi-Public', '15', '1'),
	('Private', '1', '1');

-- Default roles --
INSERT INTO `roles` (`role_name`, `role_perm_flags`, `role_creator_user_id`) values
	('None', '0', '1'),
	('Guest', '1', '1' ),
	('Viewer', '15', '1' ),
	('Contributor', '1343', '1' ),
	('Admin', '9223372036854779903', '1' );


-- Default services --
INSERT INTO `services` (`service_type`, `service_sid`, `service_description`, `service_enabled`) VALUES
	('auth', 'http://services.mindtouch.com/deki/draft/2006/11/dekiwiki', 'Local', 1),
	('ext', 'sid://mindtouch.com/2007/12/dekiscript', 'AccuWeather', 1),
	('ext', 'sid://mindtouch.com/2007/12/dekiscript', 'AddThis', 0),
	('ext', 'sid://mindtouch.com/2007/12/dapper', 'Dapper', 1),
	('ext', 'sid://mindtouch.com/2007/12/dekiscript', 'DHtml', 1),
	('ext', 'sid://mindtouch.com/2007/12/dekiscript', 'Digg', 1),
	('ext', 'sid://mindtouch.com/2007/06/feed', 'Atom/RSS Feeds', 1),
	('ext', 'sid://mindtouch.com/2007/12/dekiscript', 'Flickr', 1),
	('ext', 'sid://mindtouch.com/2007/12/dekiscript', 'FlowPlayer', 1),
	('ext', 'sid://mindtouch.com/2007/06/google', 'Google', 0),
	('ext', 'sid://mindtouch.com/2007/06/graphviz', 'Graphviz', 0),
	('ext', 'sid://mindtouch.com/2007/12/dekiscript', 'Gravatar', 1),
	('ext', 'sid://mindtouch.com/2007/06/imagemagick', 'ImageMagick', 1),
	('ext', 'sid://mindtouch.com/2008/02/jira', 'Jira', 0),
	('ext', 'sid://mindtouch.com/2007/12/dekiscript', 'LinkedIn', 1),
	('ext', 'sid://mindtouch.com/2008/01/mantis', 'Mantis', 0),
	('ext', 'sid://mindtouch.com/2007/06/math', 'Math', 0),
	('ext', 'sid://mindtouch.com/2007/06/media', 'Multimedia', 1),
	('ext', 'sid://mindtouch.com/2007/06/mysql', 'MySql', 0),
	('ext', 'sid://mindtouch.com/2007/12/dekiscript', 'PageBus', 1),
	('ext', 'sid://mindtouch.com/2007/12/dekiscript', 'PayPal', 0),
	('ext', 'sid://mindtouch.com/2007/12/dekiscript', 'Scratch', 1),
	('ext', 'sid://mindtouch.com/2007/12/dekiscript', 'Scribd', 0),
	('ext', 'sid://mindtouch.com/2008/02/silverlight', 'Silverlight', 1),
	('ext', 'sid://mindtouch.com/2007/12/dekiscript', 'Skype', 1),
	('ext', 'sid://mindtouch.com/2007/12/dekiscript', 'Spoiler', 1),
	('ext', 'sid://mindtouch.com/2008/02/svn', 'Subversion', 0),
	('ext', 'sid://mindtouch.com/2008/05/svg', 'SVG', 1),
	('ext', 'sid://mindtouch.com/2007/12/dekiscript', 'Syntax Highlighter', 1),
	('ext', 'sid://mindtouch.com/2008/02/trac', 'Trac', 0),
	('ext', 'sid://mindtouch.com/2007/12/dekiscript', 'Twitter', 1),
	('ext', 'sid://mindtouch.com/2007/12/dekiscript', 'WidgetBox', 1),
	('ext', 'sid://mindtouch.com/2007/07/windows.live', 'Windows Live', 1),
	('ext', 'sid://mindtouch.com/2007/06/yahoo', 'Yahoo!', 1),
	('ext', 'sid://mindtouch.com/2007/12/dekiscript', 'YUI Media Player', 1),
	('ext', 'sid://mindtouch.com/2007/12/dekiscript', 'EditGrid', 1),
	('ext', 'sid://mindtouch.com/2007/12/dekiscript', 'Lightbox', 1),
	('ext', 'sid://mindtouch.com/2007/12/dekiscript', 'Quicktime', 1),
	('ext', 'sid://mindtouch.com/2007/12/dekiscript', 'Remember The Milk', 1),
	('ext', 'sid://mindtouch.com/2007/12/dekiscript', 'Zoho', 1),
	('ext', 'sid://mindtouch.com/ent/2008/05/salesforce', 'Salesforce', 0),
	('ext', 'sid://mindtouch.com/ent/2008/05/sugarcrm', 'SugarCRM', 0),
	('ext', 'sid://mindtouch.com/ext/2009/12/anychart', 'MindTouch Charts', 1),
	('ext', 'sid://mindtouch.com/ext/2009/12/anygantt', 'MindTouch Gantt Charts', 0),
	('ext', 'sid://mindtouch.com/std/2009/04/activitystream', 'Activity Stream', 1),
	('ext', 'sid://mindtouch.com/ext/2010/06/analytics.search', 'Curation Analytics (Search)', 1),
	('ext', 'sid://mindtouch.com/ext/2010/06/analytics.content', 'Curation Analytics (Content)', 1);

-- Default services config --
INSERT INTO `service_config` (`service_id`, `config_name`, `config_value`) VALUES
	('2', 'manifest', 'http://scripts.mindtouch.com/accuweather.xml'),
	('3', 'manifest', 'http://scripts.mindtouch.com/addthis.xml'),
	('5', 'manifest', 'http://scripts.mindtouch.com/dhtml.xml'),
	('6', 'manifest', 'http://scripts.mindtouch.com/digg.xml'),
	('8', 'manifest', 'http://scripts.mindtouch.com/flickr.xml'),
	('9', 'manifest', 'http://scripts.mindtouch.com/flowplayer.xml'),
	('12', 'manifest', 'http://scripts.mindtouch.com/gravatar.xml'),
	('13', 'imagemagick-path', '/usr/bin/convert'),
	('15', 'manifest', 'http://scripts.mindtouch.com/linkedin.xml'),
	('20', 'manifest', 'http://scripts.mindtouch.com/pagebus.xml'),
	('21', 'manifest', 'http://scripts.mindtouch.com/paypal.xml'),
	('22', 'manifest', 'http://scripts.mindtouch.com/scratch.xml'),
	('23', 'manifest', 'http://scripts.mindtouch.com/scribd.xml'),
	('25', 'manifest', 'http://scripts.mindtouch.com/skype.xml'),
	('26', 'manifest', 'http://scripts.mindtouch.com/spoiler.xml'),
	('29', 'manifest', 'http://scripts.mindtouch.com/syntax2.xml'),
	('31', 'manifest', 'http://scripts.mindtouch.com/twitter.xml'),
	('32', 'manifest', 'http://scripts.mindtouch.com/widgetbox.xml'),
	('35', 'manifest', 'http://scripts.mindtouch.com/yuimediaplayer.xml'),
	('36', 'manifest', 'http://scripts.mindtouch.com/editgrid.xml'),
	('37', 'manifest', 'http://scripts.mindtouch.com/lightbox.xml'),
	('38', 'manifest', 'http://scripts.mindtouch.com/quicktime.xml'),
	('39', 'manifest', 'http://scripts.mindtouch.com/rtm.xml'),
	('40', 'manifest', 'http://scripts.mindtouch.com/zoho.xml');

-- Default users --
INSERT INTO `users` (`user_name`,`user_real_name`,`user_password`,`user_newpassword`,`user_email`,`user_touched`,`user_token`,`user_role_id`,`user_active`,`user_service_id`, `user_builtin`, `user_create_timestamp`) VALUES
	('Sysop', '','','','',DATE_FORMAT(NOW(), "%Y%m%d%H%i%s"),'2158a249b6b8368a738bf81d97627be1','5','1','1','1', UTC_TIMESTAMP()),
	('Anonymous','Anonymous User','','','',DATE_FORMAT(NOW(), "%Y%m%d%H%i%s"),'','3','1','1','1', UTC_TIMESTAMP());
