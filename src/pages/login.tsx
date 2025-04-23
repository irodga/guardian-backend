// src/pages/login.tsx
import { useState } from "react";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { useRouter } from "next/router";

export default function LoginPage() {
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const router = useRouter();

  function handleLogin() {
    if (password === "superadmin") {
      localStorage.setItem("guardian-auth", "ok");
      router.push("/");
    } else {
      setError("Contraseña incorrecta");
    }
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50">
      <div className="w-full max-w-sm space-y-4 p-6 bg-white rounded-xl shadow">
        <h1 className="text-xl font-bold">Login Guardian</h1>
        <Input
          type="password"
          placeholder="Contraseña"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
        />
        <Button className="w-full" onClick={handleLogin}>Ingresar</Button>
        {error && <p className="text-sm text-red-500">{error}</p>}
      </div>
    </div>
  );
}
