<?php
require_once('gui_index.php');

class CommentsFormatter extends DekiFormatter
{
	protected $contentType = 'application/json';
	protected $requireXmlHttpRequest = true;

	private $Request;

	public function format()
	{
		$this->Request = DekiRequest::getInstance();
		$action = $this->Request->getVal( 'action' );

		switch ( $action )
		{
			case 'show':
				$this->getCommentsHtml();
				break;

			case 'post':
				$this->postComment();
				break;

			case 'edit':
				$this->editComment();
				break;

			case 'delete':
				$this->deleteComment();
				break;

			default:
				header('HTTP/1.0 404 Not Found');
				exit(' '); // flush the headers
		}
	}

	private function getCommentsHtml()
	{
		$titleId = $this->Request->getVal( 'titleId' );
		$commentCount = $this->Request->getVal( 'commentCount' );

		$title = Title::newFromID( $titleId );
		$comments = new CommentPage( $title );

		echo $comments->format( $commentCount );
	}

	private function postComment()
	{
		global $wgOut, $wgTitle;
		
		$titleId = $this->Request->getVal( 'titleId' );
		$comment = $this->Request->getVal( 'comment' );
		$showAll = $this->Request->getVal( 'showAll' );
		$commentNum = $this->Request->getVal( 'commentNum' );

		$nt = Title::newFromID( $titleId );

		//set the global title, since it's used to generate local URLs
		$wgTitle = $nt;

		$Comment = new CommentPage($nt);
		$Comment->ignoreWebRequest = true; //so the form doesn't try to populate from wgRequest
		$Comment->commenttext = $comment; //manually set the comment
		
		if ( !empty($commentNum) )
		{
			$Comment->commentnum = $commentNum;
		}
		$Comment->submit();
		
		// load comments from the API
		// and output the whole comments markup for injection back into the site
		echo $Comment->format( ($showAll == 'true') ? 'all': null );
	}

	private function editComment()
	{
		$this->disableCaching();

		$titleId = $this->Request->getVal( 'titleId' );
		$commentNum = $this->Request->getVal( 'commentNum' );
		
		$nt = Title::newFromId( $titleId );
		$Comment = new CommentPage( $nt );
		$Comment->commentnum = $commentNum;
		echo $Comment->commentForm();
	}

	private function deleteComment()
	{
		$titleId = $this->Request->getVal( 'titleId' );
		$commentNum = $this->Request->getVal( 'commentNum' );
		
		$Comment = new Comment();
		$Comment->pageid = $titleId;
		$Comment->commentnum = $commentNum;
		$Comment->delete();
		global $wgUser;
		echo( '<div class="comment-deleted">' . wfMsg('Article.Comments.comment-was-deleted', $commentNum, $wgUser->getName()) . '</div>');
	}
}

new CommentsFormatter();
