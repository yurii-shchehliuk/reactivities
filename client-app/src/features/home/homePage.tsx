import React from 'react';
import { Link } from 'react-router-dom';
import { Container } from 'semantic-ui-react';

export default function HomePage() {
  return (
    <Container style={{ marginTop: '7em' }}>
      <h1>Home page</h1>
      <h1>
        Go to <Link to='/activities'>activities</Link>
      </h1>
    </Container>
  );
}
