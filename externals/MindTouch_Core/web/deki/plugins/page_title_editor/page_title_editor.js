
$(function() {
	var titleId = 'deki-page-title';
	var pluginFormatter = 'page_title_editor';

	var $editor = $('#' + titleId);
	// view
	var $view = $editor.find('.state-view');
	var $viewTitle = $editor.find('.state-view span.title');
	// hover
	var $hover = $editor.find('.state-hover');
	var $hoverTitle = $editor.find('.state-hover span.title');
	// edit
	var $edit = null;
	var defaults = {
		title: '',
		name: '',
		type: ''
	};

	// redirected pages cannot have their titles update - this is just asking for end-user confusion
	if (Deki.FollowRedirects) {
		// hook hover events
		$viewTitle.hoverIntent({
			sensitivity: 5,
			interval: 200,
			over: hoverOver
		});
		
		$hover.hover(null, hoverOut);
		$editor.find('span.title').dblclick(showEditor);
		$editor.find('.state-hover .edit').click(showEditor);
	}

	// events
	function hoverOver() {
		if (!$hoverTitle.data('deki.positioned')) {
			$hoverTitle.data('deki.positioned', true);
			
			$editor.css('position', 'relative');
			$hover.css({
				'visibility': 'hidden',
				'display': 'block'
			});
			
			// determine offset before adding class
			var ptOffset = $view.offset();
			var ctOffset = $hoverTitle.offset();
			
			// note: equal padding required
			var padding = ($hoverTitle.outerWidth() - $hoverTitle.width()) / 2;

			// determine shift
			var adjLeft = parseInt(ptOffset.left - ctOffset.left);
			var adjTop = parseInt(ptOffset.top - ctOffset.top);

			$hover.css('left', adjLeft - padding);
			$hover.css('top', adjTop - padding);
			
			$editor.css('position', '');
			$hover.css({
				'visibility': 'visible',
				'display': ''
			});
		}

		$editor.addClass('ui-state-hover');
	};
	
	function hoverOut() {
		$editor.removeClass('ui-state-hover');
	};
	
	function showEditor() {
		if (!_loadEditor())
			return;

		// clear the messages
		Deki.Ui.EmptyFlash(); 
		
		// set the default title
		$edit.find('.edit-title').val(defaults.pageTitle);

		// show edit
		$editor.addClass('ui-state-edit');

		if (defaults.pathType == 'custom') {
			_unlinkTitle();
		} else {
			if (defaults.pathType == 'fixed') {
				$edit.addClass('ui-state-fixed');
			}

			_linkTitle();
		}

		// select!
		$edit.find('.edit-title').focus();
		$edit.find('.edit-title').select();
	};

	function hideEditor() {
		$editor.removeClass('ui-state-edit');
	};
	
	function toggleTitleLink() {
		if (_isLinked()) {
			_unlinkTitle();
		} else {
			_linkTitle();
		}
	};

	function updateTitle() {
		// make the title change
		
		// disable the submit button
		$edit.find('.edit-update').attr("disabled","disabled"); 
		
		var editor = typeof(CKEDITOR) == 'object'; 
		
		var fields = {
			pageId: Deki.PageId,
			action: 'update',
			title: $edit.find('.edit-title').val(), 
			inlinerefresh: editor
		};

		// user is providing a custom path name
		if (!_isLinked()) {
			fields.name = $edit.find('.edit-path').val();
		}

		var options = {
			type: 'post',
			data: fields,
			success: function(data) {
				// update defaults
				defaults.pageTitle = data.body.title;
				defaults.pageName = data.body.name;
				
				if (editor) {
					Deki.Ui.Flash(data.message);
					// set the new display title
					_setTitle(defaults.pageTitle);
					hideEditor();
					
					$edit.find('.edit-update').removeAttr("disabled"); 
				}
				else {
					window.location = data.body.uri; 
				}
			}, 
			// error doesn't fall through
			complete: function() {
				$edit.find('.edit-update').removeAttr("disabled");
			}
		};

		Deki.Plugin.AjaxRequest(pluginFormatter, options);
		return false;
	};

	// helpers
	function _isLinked() {
		return !$edit.hasClass('ui-state-unlinked');
	};

	function _setTitle(pageTitle) {
		$viewTitle.text(pageTitle);
		$hoverTitle.text(pageTitle);
	};

	function _linkTitle() {
		$edit.removeClass('ui-state-unlinked');
		$edit.find('.edit-title').focus();
		$edit.find('.edit-title').select();
	};
	
	function _unlinkTitle() {
		if (defaults.pathType == 'fixed')
			return false;

		// set the defaults
		$edit.find('.edit-path').val(defaults.pathName);

		$edit.addClass('ui-state-unlinked');
		$edit.find('.edit-title').focus();
		$edit.find('.edit-path').select();
	};
	
	function _loadEditor() {
		if ($editor.data('loaded')) {
			return true;
		}

		// load the editing html
		var options = {
			data: {
				pageId: Deki.PageId,
				redirects: Deki.FollowRedirects
			},
			success: function(data) {
				// default title
				defaults.pageTitle = data.body.title;
				// default path
				defaults.pathName = data.body.name;
				// default link state
				defaults.pathType = data.body.type;
				// add the edit html
				$editor.append(data.body.html);

				$edit = $editor.find('.state-edit');
				$editor.data('loaded', true);

				_hookEditEvents();
				showEditor();
			}
		};

		Deki.Plugin.AjaxRequest(pluginFormatter, options);
		return false;
	};
	
	function _hookEditEvents() {
		var $textInputs = $edit.find('input[type=text]');
		var $editPath = $edit.find('.edit-path');
		var $toggleButton = $edit.find('a.toggle-link');
		var $updateButton = $edit.find('.edit-update');
		var $cancelButton = $edit.find('a.cancel');
		
		// edit events
		$cancelButton.click(hideEditor);
		$toggleButton.click(toggleTitleLink);
		$updateButton.click(updateTitle);
		
		$textInputs.keypress(function(e) {
			switch (e.keyCode) {
				case 13: // enter
					updateTitle();
					return false;
				case 27: // escape
					hideEditor();
					return false;
			}
		});
	};
});
