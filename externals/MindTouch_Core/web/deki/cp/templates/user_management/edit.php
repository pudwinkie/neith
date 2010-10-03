<?php $this->includeCss('users.css'); ?>
<?php $this->includeJavaScript('externalauth.js'); ?>
<?php $this->set('template.subtitle', $this->msg('Users.edit.title')); ?>
<?php $this->set('template.action.search', $this->get('search-form.action')); ?>

<form method="post" action="<?php $this->html('edit-form.action'); ?>" class="edit padding">
	<?php if ($this->has('user.fullname')) : ?>
	<h3>
		<?php $this->html('user.fullname'); ?>
	</h3>
	<?php endif; ?>

	<div class="field">
		<?php echo $this->msg('Users.form.username'); ?> <small><?php echo $this->msg('Users.form.required'); ?></small><br/>
		<?php echo DekiForm::singleInput('text', 'name', $this->get('user.name'), array('disabled' => !$this->get('user.isInternal') || $this->get('user.isAnonymous'))); ?>
	</div>

	<div class="field">
		<?php echo $this->msg('Users.form.email'); ?><br/>
		<?php echo DekiForm::singleInput('text', 'email', $this->get('user.email')); ?>
	</div>
	
	<div class="passwords">
		<div class="field password">
			<?php echo $this->msg('Users.form.password'); ?><br/>
			<?php echo DekiForm::singleInput('password', 'password'); ?>
		</div>
	
		<div class="field verify">
			<?php echo $this->msg('Users.form.password.verify'); ?><br/>
			<?php echo DekiForm::singleInput('password', 'password_verify'); ?>
		</div>
		<div class="password-instructions"><?php echo $this->msg('Users.form.password.instructions'); ?></div>
	</div>
			
	<div class="field">
		<?php echo $this->msg('Users.form.role'); ?><br/>
		<?php $this->html('form.role-select'); ?>
	</div>

	<div class="field status">
		<?php echo $this->msg('Users.form.status'); ?><br/>
		<?php $this->html('form.status'); ?>
	</div>

	<?php $this->html('form.auth-section'); ?>

	<?php if ($this->get('form.group-boxes.count') > 0) : ?>
		<div class="select-title"><?php echo $this->msg('Users.form.select-title'); ?></div>
		<div class="<?php echo $this->get('form.group-boxes.count') > 10 ? 'groups groups-large' : 'groups' ?>">
			<?php $this->html('form.group-boxes'); ?>
		</div>
	<?php endif; ?>
	
	<div class="submit">
		<?php echo DekiForm::singleInput('button', 'submit', 'submit', array(), $this->msg('Users.edit.button')); ?> 
		<span class="or"><?php echo $this->msgRaw('Users.form.cancel', $this->get('edit-form.back'));?></span>
	</div>	
</form>