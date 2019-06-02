DROP DATABASE IF EXISTS `cms_update`;
CREATE DATABASE IF NOT EXISTS `cms_update` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci */;
USE `cms_update`;

CREATE TABLE IF NOT EXISTS `tb_message`
(
  `message_id` BIGINT(20) NOT NULL AUTO_INCREMENT,
  `message_text`  MEDIUMTEXT NOT NULL,
  PRIMARY KEY (`message_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

DELIMITER //
CREATE PROCEDURE sp_InsertMessage
(
	IN message_text MEDIUMTEXT
) DETERMINISTIC
BEGIN
	DECLARE message_count BIGINT;
	DECLARE lowest_message_id BIGINT;

	SELECT
		COUNT(message_id)
	INTO
		message_count
	FROM
		tb_message;

	IF message_count > 10000 THEN
		SELECT
			message_id
		INTO
			lowest_message_id
		FROM
			tb_message
		ORDER BY
			message_id DESC
		LIMIT 1
		OFFSET 5000;

		DELETE
			m
		FROM
			tb_message m
		WHERE
			m.message_id < lowest_message_id;
	END IF;

	INSERT INTO
		tb_message
	(
		message_text
	) VALUES (
		message_text
	);
END//
DELIMITER ;


DELIMITER //
CREATE PROCEDURE sp_GetLastMessageId
(
	OUT message_id BIGINT
) DETERMINISTIC
BEGIN
	SELECT
		m.message_id
	INTO
		message_id
	FROM
		tb_message m
	ORDER BY
		m.message_id DESC
	LIMIT 1;
END//
DELIMITER ;


DELIMITER //
CREATE PROCEDURE sp_GetNextMessage
(
	IN message_id BIGINT UNSIGNED
) DETERMINISTIC
BEGIN
	SELECT
		m.message_id,
		m.message_text
	FROM
		tb_message m
	WHERE
		m.message_id > message_id
	ORDER BY
		m.message_id
	LIMIT 1;
END//
DELIMITER ;

