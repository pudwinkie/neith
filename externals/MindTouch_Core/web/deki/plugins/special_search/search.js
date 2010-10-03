
$(function() {
	$('#deki-search-results a.go').click(function(e) {
		var linkUrl = Deki.Plugin.SpecialSearch._lastLink;
		var trackUrl = Deki.Plugin.SpecialSearch._lastTracker;
		
		var middleClick = (e.which == 2);
		if (e.altKey || e.metaKey || e.shiftKey || middleClick)
			linkUrl = null;
		
		var handler = function() {
			if (linkUrl)
				document.location = linkUrl;
		};
		
		$.ajax({
			type: 'post',
			url: trackUrl,
			async: false,
			success: handler,
			error: handler
		});
	});
});

if (typeof Deki == "undefined")
	var Deki = {};
if (typeof Deki.Plugin == "undefined")
	Deki.Plugin = {};
Deki.Plugin.SpecialSearch = {};

Deki.Plugin.SpecialSearch._lastLink = null;
Deki.Plugin.SpecialSearch._lastTracker = null;

function __deki_search_results(e, url, track) {
	// register the result urls
	Deki.Plugin.SpecialSearch._lastLink = url;
	Deki.Plugin.SpecialSearch._lastTracker = track;
};
