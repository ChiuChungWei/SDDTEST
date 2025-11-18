export interface LoginRequest {
  adAccount: string;
  password: string;
}

export interface LoginResponse {
  success: boolean;
  token: string;
  user: UserResponse;
  expiresIn: number;
}

export interface UserResponse {
  id: number;
  adAccount: string;
  name: string;
  email: string;
  role: string;
}
