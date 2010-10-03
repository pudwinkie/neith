jQuery.fn.extend({
   check: function() {
     return this.each(function() { this.checked = true; });
   },
   uncheck: function() {
     return this.each(function() { this.checked = false; });
   }
});

jQuery.extend({
	htmlEncode : function(html)
	{
		if ( !html )
			return '';
	
		html = html.replace( /&/g, '&amp;' );
		html = html.replace( /</g, '&lt;' );
		html = html.replace( />/g, '&gt;' );
	
		return html;
	},
	
	htmlDecode : function(html)
	{
		if ( !html )
			return '' ;
	
		html = html.replace( /&gt;/g, '>' );
		html = html.replace( /&lt;/g, '<' );
		html = html.replace( /&amp;/g, '&' );
	
		return html;
	}
});

(function($)
{
	/* defaultValue plugin */
	$.fn.defaultValue = function() 
	{
		var elements = this;
		var defaultArgs = arguments;
		
		return elements.each(function(index)
		{
			var $el = $(this);
			var defVal = defaultArgs[index] || $el.attr('title');
			var defClass = 'deki-default-value'; // make an arg?

			if ($el.val() == '')
			{ // initialize only if no value
				$el.val(defVal).addClass(defClass);
			}

			$el.focus(function()
				{
					if ($el.hasClass(defClass))
					{
						$el.val('').removeClass(defClass);
					}
				})
				.blur(function()
				{
					if ($el.val() == '')
					{
						$el.val(defVal).addClass(defClass);
					}
				})
				// make sure we don't submit the default
				.parents('form:first').submit(function()
				{
					if ($el.hasClass(defClass))
					{
						$el.val('');
					}
				})
			; // end $el
		});
	};
	/* /defaultValue plugin */
	
	/* editable plugin */
	$.fn.editable = function(options)
	{ 	
		var defaults = {
			onDisplayValue: null,		// called when clicked
			url: null,					// ajax endpoint
			method: 'post',
			dataType: 'json',
			field: 'text', 				// name of the text post field
			fields: {}, 				// additional post fields
			onGenerateRequest: null,	// allow field configuration
			onSuccess: null, 			// ajax handlers
			onError: null,
			editingClass: 'editing',	// editing class
			savingClass: 'saving'		// ajax save class
		};  
		var options = $.extend(defaults, options);
	
	    return this.each(function()
	    {	
			var $this = $(this);
			var $input = null;
			var value = $this.text();
	
			$this.click(function()
			{
				// check if the input is already created
				if ($input)
					return false;
				
				$this.html('<input type="text" value="" />');
				$this.addClass(options.editingClass);
			   	
				var displayValue = value;
				if (options.onDisplayValue)
					displayValue = options.onDisplayValue($this, displayValue);

				$input = $this.find('input').attr('value', displayValue);
				$input.focus().select();
				
				// events
				$input.blur(function() { cancelEdit(value); });
				
				$input.keydown(function(e)
				{
					// esc
					if (e.which === 27)
						$input.blur();
					// enter/tab
					if (e.which === 13 || e.which === 9) {
			    		e.preventDefault();
						saveEdit();
			    	}
				});
				
				// helpers
				function cancelEdit(setValue)
				{
					if (setValue)
						value = setValue;
					
					$this.text(value);
					$this.removeClass(options.editingClass);
					$input = null;
				}
				
				function saveEdit()
				{
					var request = options;
					request.fields = request.fields || {};
					request.fields[request.field] = $input.val();
	
					if (options.onGenerateRequest) {
						options.onGenerateRequest($this, request);
					}
	
					if (!request.url)
						throw 'No ajax endpoint configured!';
					
					$input.addClass(request.savingClass);
					$.ajax({
					    type: request.method,
					   	data: request.fields,
					   	dataType: request.dataType,
			    		url: request.url,
			    		
			    		success: function(data)
			    		{
							$input.removeClass(request.savingClass);
			    			if (options.onSuccess)
			    			{
			    				cancelEdit(options.onSuccess($this, value, data));
			    			}
			    			else
			    			{
			    				cancelEdit($input.val());
			    			}
					    },
	
						error: function(xhr, textError)
						{
					    	$input.removeClass(request.savingClass);
					    	if (options.onError)
					    	{
					    		options.onError($this, value, xhr, textError);
					    	}
					    	else
					    	{
					    		$this.html(textError);
					    	}
						}
			  		});					
				}
				
				// halt click default
				return false;
			});		  
	    });
	};
	/* /editable plugin */

})(jQuery);
