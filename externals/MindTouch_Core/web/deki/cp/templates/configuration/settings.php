<?php $this->includeCss('settings.css'); ?>

<form method="post" action="<?php $this->html('form.action');?>" class="sitesettings">
	<div class="title">
		<h3><?php echo $this->msg('Settings.basic.title.options'); ?></h3>
	</div>
	<div class="input"><?php $this->html('form.input.anonymous'); ?><br/>
	<?php $this->html('form.input.searchhighlight'); ?></div>

	<div class="title">
		<h3><?php echo $this->msg('Settings.basic.form.help'); ?></h3>
	</div>
	<div class="helpurl">http:// <?php $this->html('form.input.help'); ?></div>
	<div class="default-helpurl">Default: www.mindtouch.com/Support</div>
	
	<div class="title">
		<h3><?php echo $this->msg('Settings.basic.form.name'); ?></h3>
	</div>
	<div class="input">
		<?php $this->html('form.input.sitename');?>
	</div>

	<div class="title">
		<h3><?php echo $this->msg('Settings.basic.form.timezone'); ?></h3>
	</div>
	<div class="input">
		<?php echo DekiForm::multipleInput('select', 'timezone', $this->get('options.timezone'), $this->get('form.timezone')); ?>
	</div>

	<div class="title">
		<h3><?php echo $this->msg('Settings.basic.form.language'); ?></h3>
	</div>
	<div class="input">
		<?php $this->html('form.select.language');?><br/>
		<small><?php echo($this->msg('Settings.basic.help.translate')); ?></small>
	</div>
	
	<div class="title">
		<h3><?php echo $this->msg('Settings.basic.form.polyglot'); ?></h3>
	</div>
	
	<div class="input">
		<div class="description"><?php echo($this->msg('Settings.basic.form.polyglot.description'));?></div>
		<div class="languages"><?php $this->html('form.select.polyglot');?></div>
	</div>
	
	<div class="title">
		<h3><?php echo $this->msg('Settings.basic.form.bannedwords'); ?></h3>
	</div>
	
	<div class="input">
		<div class="description"><?php echo($this->msg('Settings.basic.form.bannedwords.description'));?></div>
		<?php $this->html('form.input.bannedwords');?>
	</div>
	
	<div class="submit">
		<?php echo DekiForm::singleInput('button', 'submit', 'submit', array(), $this->msg('Settings.basic.form.button')); ?>
	</div>
</form>