$(function() {
	/*fieldsetArray = [
	 'form-admin-title',
	 'form-title',
	 'form-dbconfig',
	 'form-adv',
	 'tell-us'
	 ];*/
	errorSets = [];
	completeSets = [];
	currentSelected = 0;

	for(var i=1;i<fieldsetArray.length;i++){
		$('#' + fieldsetArray[i]).hide();
	}
	$('div.submit').hide();
	$('#box_0').attr('class' , 'step_selected');
	$('#box_0').next().attr('class' , 'step_selected');
	$('#text-SysopEmail').blur(function() {
		fillCompanyName();
	});

	$('#config input').bind('keypress', function(e) {
		if(e.keyCode==13){
			e.preventDefault();
			if(currentSelected < (fieldsetArray.length -1)){
				nextSet(currentSelected);
			} else {
				$('#config').submit();
			}
		}
	});
	$("input:visible:enabled:first").focus();
});

function previousSet(currentSet) {
	validateFieldSet(currentSet);	//you can always go back
	$('#' + fieldsetArray[currentSet]).hide();
	$('#' + fieldsetArray[currentSet - 1] ).show();
	if(currentSet == (fieldsetArray.length -1)){
		$('div.submit').hide();
	}
	navLabel(currentSet, (currentSet-1));
	currentSelected = currentSet - 1;
}

function nextSet(currentSet) {
	var form_is_valid = validateFieldSet(currentSet);
	if(!form_is_valid){
		return;
	}
	$('#' + fieldsetArray[currentSet]).hide();
	$('#' + fieldsetArray[currentSet + 1] ).show();
	if (currentSet == (fieldsetArray.length - 2)){
		$('div.submit').show();
	}
	navLabel(currentSet, (currentSet+1));
	currentSelected = currentSet + 1;
}

function navLabel(previousSet, toBeSet){
	$('#box_'+previousSet).removeClass('step_selected');
	$('#box_'+previousSet).next().removeClass('step_selected');
	$('#box_'+toBeSet).addClass('step_selected');
	$('#box_'+toBeSet).next().addClass('step_selected');
}

function gotoItem(nextSet) {
	if(nextSet != currentSelected){
		var form_is_valid = validateFieldSet(currentSelected);

		if(!form_is_valid && (nextSet > currentSelected) ){
			return;
		}
		$('#' + fieldsetArray[currentSelected]).hide();
		$('#' + fieldsetArray[nextSet] ).show();
		if(nextSet < (fieldsetArray.length -1)){
			$('div.submit').hide();
		}else{
			$('div.submit').show();
		}
		navLabel(currentSelected, nextSet);
		currentSelected = nextSet;
	}
}

function validateFieldSet(currentSet) {
	var valid = true;
	var fieldset = fieldsetArray[currentSet];
	var inputs = $('#' + fieldset + ' .table :input');

	inputs.each(function(n) {
		var input_id = (inputs[n]).id;
		var input_val = $('#'+input_id).val();
		var error_name = 'invalid_' + (inputs[n]).name;
		if( input_val && (input_id == 'text-SysopEmail' || input_id == 'text-EmergencyContact') ){
			var emailValid = /^[a-zA-Z0-9_!&=`~#%'\/\$\^\|\+\?\{\}-]+(\.[a-zA-Z0-9_!&=`~#%'\/\$\^\|\+\?\{\}-]+)*@[a-zA-Z0-9]([a-zA-Z0-9_-])*(\.[a-zA-Z0-9][a-zA-Z0-9_-]*)+$/;
			if (!emailValid.test(input_val)) {
					msg = 'Please enter a valid email address';
					$('#' + error_name).html(msg);
					$('#' + error_name).show();
					valid = false;
			}else {
				$('#' + error_name).hide();
			}
		}else if(input_val && input_id == 'password-SysopPass2'){
			if(input_val != $('#password-SysopPass').val()){
				msg = 'Passwords do not match.';
				$('#' + error_name).html(msg);
				$('#' + error_name).show();
				valid = false;
			}else{
				$('#' + error_name).show();
			}
		}else if (!input_val){
			$('#' + error_name).show();
			valid = false;
		}else {
			$('#' + error_name).hide();
		}

	});

	if (!valid){
		$('#box_'+currentSet).addClass('step_error');
		$('#box_'+currentSet).next().addClass('step_error');
	} else {
		$('#box_'+currentSet).removeClass('step_error');
		$('#box_'+currentSet).next().removeClass('step_error');
	}
	return valid;
}

/** If company name is empty,
 * and the user entered a valid email,
 * use the domain of the email to fill company name
 */
function fillCompanyName() {
	if(!$('#text-Sitename').val()){
		var emailValid = /^[a-zA-Z0-9_!&=`~#%'\/\$\^\|\+\?\{\}-]+(\.[a-zA-Z0-9_!&=`~#%'\/\$\^\|\+\?\{\}-]+)*@[a-zA-Z0-9]([a-zA-Z0-9_-])*(\.[a-zA-Z0-9][a-zA-Z0-9_-]*)+$/;
		var val = $('#text-SysopEmail').val();
		if (emailValid.test(val)) {
			var domain = val.substring((val.indexOf('@'))+1);
			domain = domain.substring(0, domain.lastIndexOf('.'));
			$('#text-Sitename').val(domain);
		}
	}
}

function validateSubmit() {
	var valid = true;
	var gotoPage = 0;
	for(var i=(fieldsetArray.length-1);i>=0;i--){
		if(!validateFieldSet(i)){
			valid = false;
			gotoPage = i;
		}
	}
	if(!valid){
		gotoItem(gotoPage);
	}
	return valid;
}
