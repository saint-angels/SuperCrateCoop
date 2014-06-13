﻿using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	public float maxSpeed  = 10f;
	public float jumpForce = 700;
	public bool facingLeft = true;
	public Transform groundCheck;
	public LayerMask whatIsGround;
	public GameObject minePrefab;

	public GameObject bulletPrefab;
	public Transform bulletSpawnPoint;
	public float fireRate = 10f;

	float lastFireTime = -1f;
	bool grounded = false;
	float groundRadius = 0.2f;
	float move = 0;
	Animator anim;

	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator> ();
	}

	void FixedUpdate () {
		grounded = Physics2D.OverlapCircle (groundCheck.position, groundRadius, whatIsGround);
		anim.SetBool ("Ground", grounded);
		
		anim.SetFloat ("vSpeed", rigidbody2D.velocity.y);

		if (networkView.isMine)
			networkView.RPC("setMoveValue", RPCMode.All, Input.GetAxis ("Horizontal"));
		
		anim.SetFloat ("Speed", Mathf.Abs(move));
		rigidbody2D.velocity = new Vector2 (move * maxSpeed, rigidbody2D.velocity.y);
		if (move < 0 && !facingLeft)
			Flip ();
		else if (move > 0 && facingLeft)
			Flip ();
	}

	void Update () {
		if(grounded && Input.GetButtonDown("Jump") && networkView.isMine)
			networkView.RPC("jump", RPCMode.All);


		if (Input.GetButton("Fire1") && anim.GetBool("Ground") && move == 0 && networkView.isMine) {
			if (Time.time > lastFireTime + 1 / fireRate){ 
				networkView.RPC("Shoot", RPCMode.All);
				lastFireTime = Time.time;
			}
		} else if (anim.GetBool("Shooting") && networkView.isMine) {
			networkView.RPC("StopShooting", RPCMode.All);
		} else if (Input.GetButton("Fire2") && anim.GetBool("Ground") && move == 0 && networkView.isMine) {
			networkView.RPC("ThrowMine", RPCMode.All);
		}
	}

	[RPC]
	void Shoot () {
		anim.SetBool("Shooting", true);
		GameObject bullet = Network.Instantiate (bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation, 1) as GameObject;
		Vector3 bulletScale = bullet.transform.localScale;
		if (!facingLeft) {
			bullet.GetComponent<Bullet> ().shootingtLeft = false;
			bulletScale.x *= -1;
			bullet.transform.localScale = bulletScale;
		}
	}

	[RPC]
	void ThrowMine() {
		GameObject mine = Network.Instantiate (minePrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation, 1) as GameObject;
	}

	[RPC]
	void StopShooting() {
		anim.SetBool("Shooting", false);
	}

	[RPC]
	void setMoveValue(float move) {
		this.move = move;
	}

	[RPC]
	void jump() {
		anim.SetBool("Ground", false);
		rigidbody2D.AddForce(new Vector2(0, jumpForce));
	}

	void Flip () {
		facingLeft = !facingLeft;
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}
}
