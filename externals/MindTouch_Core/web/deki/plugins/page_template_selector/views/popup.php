<style type="text/css">#deki-pagetemplates-popup.loading { display: none; }</style>
<form class="special-page-form">
<div id="deki-pagetemplates-popup" class="loading">
	<ul id="deki-pagetemplates-layouts">
		<li class="page-item page-item-default">
				<?php $this->html('templates.default'); ?>
		</li>
	<?php
		$templates = $this->get('templates.rendered');
		
		foreach ($templates as $template) :
		?>
			<li class="page-item">
				<?php echo $template; ?>
			</li>
		<?php endforeach;

		for ($i = count($templates); $i < 8; $i++) :
			?>
			<li class="page-item-empty"></li>
		<?php endfor; ?>
	</ul>
	<div id="deki-pagetemplates-message">
			<?php $this->html('templates.available'); ?>
	</div>
	
	<div id="footer">
		<div class="deki-pagetemplates-footer-content">
			<div id="deki-pagetemplates-help">
				<?php $this->html('templates.help'); ?>
			</div>
			
			<a href="" id="deki-pagetemplates-create" target="_parent" class="button">
				<?php echo $this->msg('Page.PageTemplateSelector.label.create'); ?>
			</a>
		</div>
	</div>
</div>
</form>

<script type="text/javascript">
// @note kalida: thickbox has a delay before display, so cannot set focus immediately
$(function(){ setTimeout("$('#deki-pagetemplates-create').focus()", 100); });
</script>
