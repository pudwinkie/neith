<?php $this->includeCss('settings.css'); ?>

<form method="post" action="<?php $this->html('form.action');?>" class="editorconfig">
	<div class="dekiFlash">
		<ul class="info first">
			<li><?php echo($this->msg('EditorConfig.description'));?></li>
		</ul>
	</div>
	
	<div class="field">
		<?php echo $this->msg('EditorConfig.form.toolbar'); ?><br/>
		<?php echo $this->html('form.select'); ?>
	</div>
	
	<div class="field">
		<?php echo $this->msg('EditorConfig.form.config'); ?><br/>
		<?php echo DekiForm::singleInput('textarea', 'config', $this->get('form.config'), array('class' => 'resizable')) ; ?>
	</div>
	
	<div class="submit">
		<?php echo DekiForm::singleInput('button', 'submit', 'submit', array(), $this->msg('EditorConfig.form.button')); ?>
	</div>
</form>