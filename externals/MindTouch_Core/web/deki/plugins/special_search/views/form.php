
<div class="deki-search-form">
	<form method="get" action="<?php $this->html('form.action'); ?>">
		<div class="inputs">
			<?php if ($this->has('form.queryId')) : ?>
				<?php echo DekiForm::singleInput('hidden', 'qid', $this->get('form.queryId')); ?>
			<?php endif; ?>
			<?php echo DekiForm::singleInput('text', 'search', null, array('autocomplete' => true)); ?>
			<?php echo DekiForm::multipleInput('select', 'ns', $this->get('form.namespaces'), $this->get('form.namespace')); ?>
			
			<input type="submit" value="<?php echo $this->msg('Page.Search.submit-search'); ?>" />
		</div>

		<?php if ($this->get('form.filters')) : ?>
		<div class="filters">
			
			<?php if ($this->has('commercial')): ?>
				<span class="sort"><?php echo $this->msg('Page.Search.sort'); ?></span>
				<ul class="sortby">
					<li class="<?php $this->html('sort.ranking'); ?>">
						<a href="<?php $this->html('href.sort.ranking'); ?>" <?php echo !$this->has('commercial') ? 'class="disabled-commercial"': ''; ?>><?php echo $this->msg('Page.Search.sort.ranking'); ?></a>
					</li>
					
					<li class="<?php $this->html('sort.title'); ?>">
						<a href="<?php $this->html('href.sort.title'); ?>" <?php echo !$this->has('commercial') ? 'class="disabled-commercial"': ''; ?>><?php echo $this->msg('Page.Search.sort.title'); ?></a>
					</li>
					<li class="<?php $this->html('sort.modified'); ?>">
						<a href="<?php $this->html('href.sort.modified'); ?>" <?php echo !$this->has('commercial') ? 'class="disabled-commercial"': ''; ?>><?php echo $this->msg('Page.Search.sort.modified'); ?></a>
					</li>
				</ul>
			<?php elseif ($this->has('commercial.messaging')) : ?>
				<div class="results-ranking">
					<a href="<?php echo $this->get('commercial.url'); ?>"><?php echo $this->msg('Page.Search.commercial'); ?></a>
				</div>
			<?php endif; ?>
			
			
			<?php if ($this->has('form.languages')) : ?>
			<div class="language">
				<?php echo $this->msg('Page.Search.form.language'); ?>
				<?php echo DekiForm::multipleInput('select', 'language', $this->get('form.languages'), $this->get('form.language')); ?>
				<input type="submit" value="<?php echo $this->msg('Page.Search.language'); ?>" />
			</div>
			<?php endif; ?>
		</div>
		<?php endif; ?>
	</form>
</div>
