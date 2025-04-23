// src/pages/login.tsx
import { useState } from "react";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { useRouter } from "next/router";

export default function LoginPage() {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const router = useRouter();

  async function handleLogin() {
    const res = await fetch("/auth/login", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ username, password })
    });

    const data = await res.json();
    if (res.ok && data.auth === "ok") {
      localStorage.setItem("guardian-auth", "ok");
      localStorage.setItem("guardian-user", data.username);
      localStorage.setItem("guardian-role", data.isAdmin ? "admin" : "user");
      router.push("/");
    } else {
      setError("Credenciales inválidas");
    }
  }

  function handleLogout() {
    localStorage.removeItem("guardian-auth");
    localStorage.removeItem("guardian-user");
    localStorage.removeItem("guardian-role");
    router.push("/login");
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50">
      <div className="w-full max-w-sm space-y-4 p-6 bg-white rounded-xl shadow">
        <h1 className="text-xl font-bold">Iniciar sesión</h1>
        <Input
          type="text"
          placeholder="Usuario"
          value={username}
          onChange={(e) => setUsername(e.target.value)}
        />
        <Input
          type="password"
          placeholder="Contraseña"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
        />
        <Button className="w-full" onClick={handleLogin}>Ingresar</Button>
        <Button variant="outline" className="w-full" onClick={handleLogout}>Cerrar sesión</Button>
        {error && <p className="text-sm text-red-500">{error}</p>}
      </div>
    </div>
  );
}