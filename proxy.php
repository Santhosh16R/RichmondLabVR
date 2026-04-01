<?php

header("Content-Type: application/json");

$data = json_decode(file_get_contents("php://input"), true);

$url = $data["url"];
$method = $data["method"];
$body = $data["body"];

$ch = curl_init($url);

curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
curl_setopt($ch, CURLOPT_CUSTOMREQUEST, $method);

$headers = [
    "Content-Type: application/json",
    "ngrok-skip-browser-warning: true"
];

if ($body) {
    curl_setopt($ch, CURLOPT_POSTFIELDS, $body);
}

curl_setopt($ch, CURLOPT_HTTPHEADER, $headers);

$response = curl_exec($ch);

if (curl_errno($ch)) {
    echo json_encode([
        "error" => curl_error($ch)
    ]);
    exit;
}

curl_close($ch);

echo $response;
