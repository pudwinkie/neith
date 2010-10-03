<?php
/*
 * MindTouch Deki - enterprise collaboration and integration platform
 * Copyright (C) 2006 MindTouch, Inc.
 * www.mindtouch.com  oss@mindtouch.com
 *
 * For community documentation and downloads visit www.opengarden.org;
 *  please review the licensing section.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along
 * with this program; if not, write to the Free Software Foundation, Inc.,
 * 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.
 * http://www.gnu.org/copyleft/gpl.html
 */


class DekiRole implements IDekiApiObject
{
	static $cache = array();

	private $id = null;
	private $name = null;
	private $operations = array();


	static function getSiteList()
	{
		global $wgAdminPlug;
		$Plug = $wgAdminPlug->At('site', 'roles');
		$Result = $Plug->Get();

		if (!$Result->isSuccess())
		{
			throw new Exception('Could not load site roles');
		}

		$permissions = $Result->getAll('body/roles/permissions');
		
		$roles = array();
		foreach ($permissions as $perm)
		{
			$Role = self::newFromArray($perm);
			$roles[$Role->getId()] = $Role;
		}

		return $roles;
	}
	
	/*
	 * Specialty function that returns all the possible role ops
	 */
	static function getSiteOperations()
	{
		global $wgAdminPlug;
		$Plug = $wgAdminPlug->At('site', 'operations');
		$Result = $Plug->Get();

		if (!$Result->isSuccess())
		{
			return null;
		}

		$operations = $Result->getVal('body/operations/#text');
		
		return explode(',', $operations);
	}

	static function &newFromId($id)
	{
		$Role = self::load($id);
		return $Role;
	}

	static function &newFromText($roleName)
	{
		$Role = self::load($roleName, true);
		return $Role;
	}

	static function &newFromArray(&$result)
	{
		if (isset($result['role']))
		{
			$Role = new DekiRole($result['role']['@id'], $result['role']['#text'], $result['operations']['#text']);
		}
		else
		{
			// user does not have an actual role
			$Role = new DekiRole(0, '');
		}
		
		return $Role;
	}

	private static function load($id, $fromName = false)
	{
		if (!isset(self::$cache[$id]) || $fromName)
		{
			global $wgAdminPlug;
				
			$Plug = $wgAdminPlug->At('site', 'roles')->At(($fromName ? '=' : '') . $id);
			
			$Result = $Plug->Get();
			if (!$Result->isSuccess())
			{
				return null;
			}
			
			$result = $Result->getVal('body/permissions');

			$Role = self::newFromArray($result);
			self::$cache[$Role->getId()] = $Role;
		}
		
		return self::$cache[$id];
	}


	public function __construct($id, $name, $operations = null)
	{
		$this->id = $id;
		$this->name = $name;
		$this->setOperations($operations);
	}


	public function getId()			{ return $this->id; }
	public function getName()		{ return $this->name; }
	public function getOperations()	{ return implode(',', $this->operations); }
	
	public function setOperations($operations = null)
	{
		if (is_array($operations))
		{
			$this->operations = $operations;
		}
		else
		{
			$this->operations = empty($operations) ? array() : explode(',', $operations);
		}
	}

	// checks if a role has a certain permission
	public function has($perm) { return in_array(strtoupper($perm), $this->operations); }

	public function create()
	{
		global $wgAdminPlug;

		$Plug = $wgAdminPlug->At('site', 'roles', '='.$this->getName());
		
		return $Plug->Put($this->toArray());
	}

	public function update()
	{
		global $wgAdminPlug;

		$Plug = $wgAdminPlug->At('site', 'roles', $this->getId());
		
		return $Plug->Put($this->toArray());
	}


	public function toArray()
	{
		$role = array(
			'operations' => $this->getOperations()
		);

		$role = array('permissions' => $role);
		return $role;
	}

	public function toXml()
	{
		return encode_xml($this->toArray());
	}

	public function toHtml()
	{
		// there was some debate whether these are "true" abbrs, which is why we have the span.abbr
		return sprintf('<span title="%s" class="abbr">%s</span>', htmlspecialchars($this->getOperations()), htmlspecialchars($this->getName()));
	}
}
