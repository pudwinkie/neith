<?php $this->includeCss('dashboard.css'); ?>

<div class="activation">
	<div class="title">
		<h3><?php echo $this->msg('Dashboard.activate'); ?></h3>
	</div>
	<div class="indent staus-<?php $this->html('product.type');?>">
		<div class="status-active"></div>
		<?php if ($this->get('is.commercial')): ?>
		<div class="status"><?php echo $this->msg('Dashboard.activated'); ?></div>
		<div class="expires"><?php echo $this->msg('Dashboard.expiration', $this->get('product.expiration')); ?></div>
		<?php endif; ?>
		<div class="version"><?php echo $this->msgRaw('Dashboard.version', $this->get('product.name'), $this->get('product.version')); ?></div>
		<div class="version-details"><a href="<?php $this->html('page.versions');?>"><?php echo $this->msg('Dashboard.version.details'); ?></a></div>
		<?php if ($this->has('upgradetext')): ?>
		<div class="version-update"><?php $this->html('upgradetext'); ?></div>
		<?php endif; ?>
	</div>
	
</div>

<div class="info">
	<div class="title">
		<h3><?php echo $this->msg('Dashboard.info'); ?></h3>
	</div>
	<div class="indent">
		<div class="license"><?php echo $this->msg('Dashboard.activate.contact'); ?></div>
		<div class="more"><?php echo $this->msg('Dashboard.deki.help'); ?></div>
	</div>
</div>

<div class="credits">
	<div class="title">
		<h3><?php echo $this->msg('Dashboard.credits'); ?></h3>
	</div>
	<div class="indent">
		<div class="thanks"><?php echo $this->msg('Dashboard.credit'); ?></div>
	</div>
	<div class="logos">
		<div class="debian column"><a href="http://debian.org"><span class="text">Debian</span></a></div>
		<div class="block column">
			<div class="mysql"><a href="http://www.mysql.com"><span class="text">MySQL</span></a></div>
			<div class="php"><a href="http://php.net"><span class="text">PHP</span></a></div>
		</div>
		<div class="block column">
			<div class="lucene"><a href="http://lucene.apache.org/java/docs/"><span class="text">Apache Lucene</span></a></div>
			<div class="apache"><a href="http://www.apache.org"><span class="text">Apache HTTP Server Project</span></a></div>
		</div>
		<div class="block column">
			<div class="mono"><a href="http://www.mono-project.com/Main_Page"><span class="text">Mono</span></a></div>
			<div class="jquery"><a href="http://jquery.com"><span class="text">jQuery</span></a></div>
		</div>
		<div class="block column">
			<div class="fckeditor"><a href="http://fckeditor.com"><span class="text">FCKeditor</span></a></div>
			<div class="prince"><a href="http://princexml.com"><span class="text">PrinceXML</span></a></div>
		</div>
	</div>
	<div class="links">
		<ul class="list">
			<li><a href="http://logging.apache.org/log4net/index.html">Apache log4net</a></li>
			<li><a href="http://www.codeproject.com/KB/miscctrl/balloonhelp.aspx">Balloon Help</a></li>
			<li><a href="http://www.ssw.uni-linz.ac.at/Research/Projects/Coco/">Coco/R</a></li>
		</ul>
		<ul class="list">	
				<li><a href="http://www.codeproject.com/KB/wtl/customautocomplete_wtl.aspx">CustomAutoComplete</a></li>
				<li><a href="http://famfamfam.com/">Famfamfam Silk Icon Set</a></li>
				<li><a href="http://www.codeplex.com/IIRF">IIRF Rewriter</a></li>
		</ul>
		<ul class="list">	
			<li><a href="http://www.imagemagick.org/script/index.php">ImageMagick</a></li>
			<li><a href="http://magpierss.sourceforge.net/">Magpie RSS</a></li>
			<li><a href="http://wiki.developer.mindtouch.com/Dream">MindTouch Dream</a></li>
		</ul>
		<ul class="list">
			<li><a href="http://phpmailer.codeworxtech.com/">PHP Mailer</a></li>
			<li><a href="http://wiki.developer.mindtouch.com/SgmlReader">SGMLReader</a></li>
			<li><a href="http://www.icsharpcode.net/OpenSource/SharpZipLib/">SharpZipLib</a></li>
		</ul>
		<ul class="list">
			<li><a href="http://swfupload.org/">SWFUpload</a></li>
			<li><a href="http://wix.sourceforge.net/">Wix</a></li>
			<li><a href="http://developer.yahoo.com/yui/">YUI</a></li>
		</ul>
	</div>
	<div class="clear"></div>
</div>