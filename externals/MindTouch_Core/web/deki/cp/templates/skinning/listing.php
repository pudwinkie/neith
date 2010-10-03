<?php
$this->includeCss('customize.css');
$this->includeJavascript('jquery.scrollTo.js');
$this->includeJavascript('customize.js');
?>

<form method="post" enctype="multipart/form-data" action="<?php $this->html('form.action'); ?>" class="logo">
	<div class="setlogo">
		<div class="title">
			<h3><?php echo($this->msg('Skinning.logo'));?></h3>
		</div>
		
		<div class="preview">
			<?php echo($this->msg('Skinning.logo.current'));?>
			<div class="logopreview" style="width: <?php $this->html('logo-maxwidth');?>px; height: <?php $this->html('logo-maxheight');?>px;">
				<span class="dimensions" style="width: <?php $this->html('logo-maxwidth');?>px; height: <?php $this->html('logo-maxheight');?>px;">
					<span><?php echo($this->msg('Skinning.logo.dimensions', $this->get('logo-maxwidth'), $this->get('logo-maxheight')));?></span>
				</span>
				<?php $this->html('logo'); ?>
			</div>
		</div>
		
		<div class="field">
			<div class="file">
				<?php echo DekiForm::singleInput('file', 'logo'); ?>
			</div>
			<small><?php echo($this->msg('Skinning.logo.description', $this->get('logo-maxwidth'), $this->get('logo-maxheight')));?></small>
			<div class="submit">
				<?php echo DekiForm::singleInput('button', 'submit', 'upload', array(), $this->msg('Skinning.logo.button')); ?>
				<?php echo DekiForm::singleInput('button', 'submit', 'default', array('class' => 'gray'), $this->msg('Skinning.logo.default')); ?>
			</div>
			<?php DekiForm::singleInput('hidden', 'action', 'logo'); ?>
		</div>
	</div>
</form>

<form method="post" action="<?php $this->html('form.action'); ?>" class="skins">
	<div class="skins">
		<div class="title">
			<h3><?php echo($this->msg('Skinning.skin'));?></h3>
		</div>
		<div class="field">
			<div id="previews" class="previews">
				<div class="thumbnails">
					<ul>
						<?php
							$skinMediumPreview = '';
							foreach ($this->get('screenshots') as $details)
							{						
								$value = $details['skin'] .'|'. $details['style'];
								$id = 'radio-skinstyle-'.$value;
								if ($details['selected'])
								{
									$skinMediumPreview = $details['medium'];
								}

								echo '<li class="'. ($details['selected'] ? 'selected-skin selected' : '') .'">';
									echo '<label for="'. $id .'" title="'. $details['skin'] .': '. $details['style'] .'">';
									echo '<img src="'. $details['small'] .'" mediumSrc="'. $details['medium'] .'" width="105" />';
									echo '</label>';
									echo DekiForm::singleInput(
										'radio', 
										'skinstyle', 
										$value, 
										array('checked' => $details['selected']),
										$details['skin'] .': '. $details['style']
									);
								echo '</li>';
							}
						?>
					</ul>
				</div>
				
				<div class="screenshot">
					<div class="label-preview"><?php echo $this->msg('Skinning.skin.preview'); ?></div>
					<div class="image" style="background-image: url(<?php echo $skinMediumPreview; ?>);"></div>
					<div class="code-links">
						<a href="<?php $this->html('url.css');?>"><?php echo $this->msg('Skinning.skin.css'); ?></a>
						<a href="<?php $this->html('url.html');?>"><?php echo $this->msg('Skinning.skin.html'); ?></a>
					</div>
				</div>
				<div class="br"></div>
			</div>
			
			<div class="submit">
				<?php echo DekiForm::singleInput('button', 'submit', 'skin', array(), $this->msg('Skinning.skin.button')); ?>
			</div>
		</div>
		
	</div>
</form>