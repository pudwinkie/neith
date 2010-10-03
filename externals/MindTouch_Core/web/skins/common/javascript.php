<?php ob_start();
global $wgUser, $wgArticle;
?>
	<script type="text/javascript">

		<?php
		echo(Skin::showEditorJavascript());
		
		# For Language.php items that need to be displayed in JS
		echo(Skin::jsLangVars());
		echo(MTMessage::ShowJavascript());
		?>

	//hook in menu clicking events to the document's body
	YAHOO.util.Event.addListener(document, "click", function () { DWMenu.BodyClick(); });
	YAHOO.util.Event.onDOMReady(function () { new clientWindow });

	<?php if ($wgArticle->getId() > 0 && $wgArticle->userCanEdit()) { ?>
		YAHOO.util.Event.onDOMReady(function() {new hookTOCLinks});
			<?php if (Skin::isViewPage()) { ?>
				YAHOO.util.Event.onDOMReady(function() { MTPageLoad.HookEditSectionIcons() });
			<?php } ?>
	<?php } ?>
	<?php
	if (Skin::isEditPage())
	{
		$Request = DekiRequest::getInstance();
		if ($wgArticle->getId() == 0 && $wgArticle->userCanCreate()
			|| $wgArticle->getId() > 0 && $wgArticle->userCanEdit() || $Request->getVal('action') == 'source')
		{
			echo('YAHOO.util.Event.onDOMReady(function() { Deki.LoadEditor(null, "' . $Request->getVal('action') . '"); });');
		}
	}
	?>
	<?php if (Skin::isPrintPage()) : ?>
	   YAHOO.util.Event.onDOMReady(function() { Print.onBodyLoad() });
	<?php endif; ?>


	<?php if ($wgArticle->getTitle()->isEditable() || $wgArticle->getTitle()->getPrefixedText() == 'Special:Search') : ?>
		YAHOO.util.Event.onDOMReady(function() {
			tb_init("a.lightbox, area.lightbox, input.lightbox, a[rel='awesomebox']");//pass where to apply thickbox
		});
	<?php endif; ?>

	<?php if ($wgArticle->getTitle()->isEditable()) : ?>
		YAHOO.util.Event.onDOMReady(function() {
			MTComments.HookBehavior();
		});
	<?php endif; ?>
	</script>
<?php
$javascript = ob_get_contents();
ob_end_clean();
?>
